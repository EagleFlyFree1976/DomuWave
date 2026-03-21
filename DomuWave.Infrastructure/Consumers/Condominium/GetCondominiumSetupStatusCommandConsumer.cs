using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Condominium;

public class GetCondominiumSetupStatusCommandConsumer
    : InMemoryConsumerBase<GetCondominiumSetupStatusCommand, CondominiumSetupStatusDto>
{
    private readonly IUserService _userService;

    public GetCondominiumSetupStatusCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<CondominiumSetupStatusDto> Consume(
        GetCondominiumSetupStatusCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condId = command.CondominiumId;
        var dto = new CondominiumSetupStatusDto { CondominiumId = condId };

        // ── 1. Unità immobiliari ──────────────────────────────────────────────
        var units = await session.Query<RealEstateUnit>()
            .Where(u => u.Condominium.Id == condId && !u.IsDeleted)
            .Select(u => new { u.Id, u.InternalNumber, u.IsActive })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var activeUnits = units.Where(u => u.IsActive).ToList();

        dto.Units.Checks.Add(new SetupCheckDto
        {
            IsOk   = units.Count > 0,
            Label  = $"{units.Count} unità censite",
            Detail = units.Count == 0 ? "Aggiungi almeno un'unità immobiliare." : null,
        });
        dto.Units.Checks.Add(new SetupCheckDto
        {
            IsOk   = activeUnits.Count > 0,
            IsWarn = units.Count > 0 && activeUnits.Count == 0,
            Label  = $"{activeUnits.Count} unità attive",
            Detail = units.Count > 0 && activeUnits.Count == 0 ? "Attiva almeno un'unità." : null,
        });
        dto.Units.Status = units.Count == 0 ? SetupSectionStatus.Error
            : activeUnits.Count == 0        ? SetupSectionStatus.Warn
            : SetupSectionStatus.Ok;

        // ── 2. Proprietari & Inquilini ────────────────────────────────────────
        if (activeUnits.Count == 0)
        {
            dto.Occupants.Status = SetupSectionStatus.Na;
            dto.Occupants.Checks.Add(new SetupCheckDto
            {
                IsOk = true, IsWarn = false,
                Label = "Nessuna unità attiva — configura prima le unità.",
            });
        }
        else
        {
            var activeUnitIds = activeUnits.Select(u => u.Id).ToList();

            var ownerUnitIds = await session.Query<UnitOwner>()
                .Where(o => activeUnitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
                .Select(o => o.Unit.Id)
                .Distinct()
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var tenantUnitIds = await session.Query<UnitTenant>()
                .Where(t => activeUnitIds.Contains(t.Unit.Id) && t.IsActive && !t.IsDeleted)
                .Select(t => t.Unit.Id)
                .Distinct()
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var withoutOwner     = activeUnits.Where(u => !ownerUnitIds.Contains(u.Id)).ToList();
            var occupiedUnitIds  = ownerUnitIds.Union(tenantUnitIds).ToHashSet();
            var withoutAny       = activeUnits.Count(u => !occupiedUnitIds.Contains(u.Id));

            dto.Occupants.Checks.Add(new SetupCheckDto
            {
                IsOk   = withoutOwner.Count == 0,
                IsWarn = withoutOwner.Count > 0,
                Label  = $"{ownerUnitIds.Count} / {activeUnits.Count} unità con proprietario",
                Detail = withoutOwner.Count > 0
                    ? $"Unità senza proprietario: {string.Join(", ", withoutOwner.Select(u => u.InternalNumber))}"
                    : null,
            });
            dto.Occupants.Checks.Add(new SetupCheckDto
            {
                IsOk   = withoutAny == 0,
                IsWarn = withoutAny > 0,
                Label  = withoutAny == 0
                    ? "Tutte le unità hanno almeno un occupante"
                    : $"{withoutAny} unità senza nessun occupante",
                Detail = withoutAny > 0 ? "Considera di aggiungere i proprietari/inquilini mancanti." : null,
            });

            dto.Occupants.Status = withoutOwner.Count == 0 ? SetupSectionStatus.Ok : SetupSectionStatus.Warn;
        }

        // ── 3. Piano dei conti ────────────────────────────────────────────────
        var accounts = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == condId && !a.IsDeleted && a.IsActive)
            .Select(a => new { a.Id, a.Type })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var hasEntrata     = accounts.Any(a => a.Type == ChartOfAccountsType.Entrata);
        var hasUscita      = accounts.Any(a => a.Type == ChartOfAccountsType.Uscita);
        var hasPatrimoniale = accounts.Any(a => a.Type == ChartOfAccountsType.Patrimoniale);

        dto.ChartOfAccounts.Checks.Add(new SetupCheckDto
        {
            IsOk   = accounts.Count > 0,
            Label  = $"{accounts.Count} conti attivi configurati",
            Detail = accounts.Count == 0 ? "Aggiungi i conti o copia da un altro condominio." : null,
        });
        dto.ChartOfAccounts.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasEntrata,
            IsWarn = !hasEntrata,
            Label  = hasEntrata ? "Conti di tipo Entrata presenti" : "Nessun conto di tipo Entrata",
            Detail = !hasEntrata ? "Aggiungi almeno un conto di tipo Entrata." : null,
        });
        dto.ChartOfAccounts.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasUscita,
            IsWarn = !hasUscita,
            Label  = hasUscita ? "Conti di tipo Uscita presenti" : "Nessun conto di tipo Uscita",
            Detail = !hasUscita ? "Aggiungi almeno un conto di tipo Uscita." : null,
        });
        dto.ChartOfAccounts.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasPatrimoniale,
            IsWarn = !hasPatrimoniale,
            Label  = hasPatrimoniale ? "Conti di tipo Patrimoniale presenti" : "Nessun conto di tipo Patrimoniale",
            Detail = !hasPatrimoniale ? "Aggiungi almeno un conto di tipo Patrimoniale." : null,
        });
        dto.ChartOfAccounts.Status = accounts.Count == 0       ? SetupSectionStatus.Error
            : (!hasEntrata || !hasUscita || !hasPatrimoniale)   ? SetupSectionStatus.Error
            : SetupSectionStatus.Ok;

        // ── 4. Tabelle millesimali ────────────────────────────────────────────
        var tables = await session.Query<MillesimalTable>()
            .Where(t => t.Condominium.Id == condId && !t.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var enabledCount = tables.Count(t => t.IsEnabled);
        var anomalyCount = 0;
        var allAssigned  = true;

        foreach (var table in tables)
        {
            var rows = await session.Query<UnitMillesimal>()
                .Where(r => r.MillesimalTable.Id == table.Id && !r.IsDeleted)
                .Select(r => new { r.Millesimal })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rowCount  = rows.Count;
            var totalCalc = rows.Sum(r => r.Millesimal);
            var hasAnomaly = table.TotalMillesimal > 0
                && Math.Abs(totalCalc - table.TotalMillesimal) > 0.01m;

            if (hasAnomaly) anomalyCount++;
            var tableAssigned = activeUnits.Count == 0 || rowCount >= activeUnits.Count;
            if (!tableAssigned) allAssigned = false;

            dto.MillesimalTables.Checks.Add(new SetupCheckDto
            {
                IsOk   = tableAssigned && !hasAnomaly,
                IsWarn = !tableAssigned || hasAnomaly,
                Label  = $"«{table.Name ?? table.Code}»: {rowCount} / {activeUnits.Count} unità valorizzate"
                    + (hasAnomaly ? " — anomalia totale" : string.Empty),
                Detail = !tableAssigned
                    ? $"{activeUnits.Count - rowCount} unità non ancora valorizzate."
                    : hasAnomaly
                        ? "Il totale calcolato non coincide con quello dichiarato."
                        : null,
            });
        }

        dto.MillesimalTables.Checks.Insert(0, new SetupCheckDto
        {
            IsOk   = tables.Count > 0,
            Label  = $"{tables.Count} tabelle millesimali create",
            Detail = tables.Count == 0 ? "Crea almeno una tabella millesimale." : null,
        });
        dto.MillesimalTables.Checks.Insert(1, new SetupCheckDto
        {
            IsOk   = enabledCount > 0,
            IsWarn = tables.Count > 0 && enabledCount == 0,
            Label  = $"{enabledCount} tabelle abilitate",
            Detail = tables.Count > 0 && enabledCount == 0 ? "Abilita almeno una tabella." : null,
        });

        dto.MillesimalTables.Status = tables.Count == 0     ? SetupSectionStatus.Error
            : enabledCount == 0 || !allAssigned || anomalyCount > 0 ? SetupSectionStatus.Warn
            : SetupSectionStatus.Ok;

        // ── 5. Esercizio fiscale ──────────────────────────────────────────────
        var today = DateTime.Today;
        var fiscalYears = await session.Query<FiscalYear>()
            .Where(fy => fy.Condominium.Id == condId && !fy.IsDeleted)
            .Select(fy => new { fy.Id, fy.Code, fy.StartDate, fy.EndDate, fy.IsActive, StatusId = fy.Status.Id })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var hasAny         = fiscalYears.Count > 0;
        var activeYear     = fiscalYears.FirstOrDefault(fy => fy.IsActive);
        var hasActive      = activeYear != null;
        var activeCoverToday = hasActive && activeYear!.StartDate <= today && today <= activeYear.EndDate;
        var anyCoversToday = fiscalYears.Any(fy => fy.StartDate <= today && today <= fy.EndDate);
        var hasOpen        = fiscalYears.Any(fy => fy.StatusId == FiscalYearStatus.Open);

        dto.FiscalYear.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasAny,
            Label  = $"{fiscalYears.Count} esercizi fiscali",
            Detail = !hasAny ? "Crea un esercizio fiscale." : null,
        });
        dto.FiscalYear.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasActive,
            IsWarn = hasAny && !hasActive,
            Label  = hasActive ? $"Esercizio attivo: «{activeYear!.Code}»" : "Nessun esercizio attivo",
            Detail = hasAny && !hasActive ? "Imposta un esercizio come attivo." : null,
        });
        dto.FiscalYear.Checks.Add(new SetupCheckDto
        {
            IsOk   = !hasActive || activeCoverToday,
            IsWarn = hasActive && !activeCoverToday,
            Label  = !hasActive      ? "Nessun esercizio attivo da verificare"
                   : activeCoverToday ? "L'esercizio attivo include la data odierna"
                   : $"L'esercizio attivo «{activeYear!.Code}» non include la data odierna ({today:dd/MM/yyyy})",
            Detail = hasActive && !activeCoverToday
                ? $"Il periodo {activeYear!.StartDate:dd/MM/yyyy}–{activeYear.EndDate:dd/MM/yyyy} non copre oggi. Controlla le date o cambia l'esercizio attivo."
                : null,
        });
        dto.FiscalYear.Checks.Add(new SetupCheckDto
        {
            IsOk   = anyCoversToday,
            IsWarn = hasAny && !anyCoversToday,
            Label  = anyCoversToday ? "La data odierna è coperta da un esercizio"
                   : "La data odierna non è coperta da nessun esercizio",
            Detail = hasAny && !anyCoversToday
                ? $"Nessun esercizio definito include {today:dd/MM/yyyy}. Crea o modifica un esercizio per coprire il periodo corrente."
                : null,
        });
        dto.FiscalYear.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasOpen,
            IsWarn = hasAny && !hasOpen,
            Label  = hasOpen ? "Esercizio in stato Aperto presente" : "Nessun esercizio in stato Aperto",
            Detail = hasAny && !hasOpen ? "Apri o crea un esercizio fiscale per l'anno corrente." : null,
        });
        dto.FiscalYear.Status = !hasAny                                       ? SetupSectionStatus.Error
            : (!hasActive || !anyCoversToday)                                  ? SetupSectionStatus.Error
            : (hasActive && !activeCoverToday) || !hasOpen                     ? SetupSectionStatus.Warn
            : SetupSectionStatus.Ok;

        // ── 6. Budget ─────────────────────────────────────────────────────────
        var budgets = await session.Query<Budget>()
            .Where(b => b.Condominium.Id == condId && !b.IsDeleted)
            .Select(b => new { b.Id, StatusId = b.Status.Id, FiscalYearId = b.FiscalYear.Id })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var hasApproved = budgets.Any(b => b.StatusId == BudgetStatus.Approved);

        // Check budget approvato per l'esercizio attivo corrente
        var activeFyId = activeYear?.Id;
        var hasApprovedForActive = activeFyId.HasValue
            && budgets.Any(b => b.FiscalYearId == activeFyId.Value
                             && (b.StatusId == BudgetStatus.Approved || b.StatusId == BudgetStatus.Closed));

        dto.Budget.Checks.Add(new SetupCheckDto
        {
            IsOk   = budgets.Count > 0,
            Label  = $"{budgets.Count} budget creati",
            Detail = budgets.Count == 0 ? "Crea un budget preventivo." : null,
        });
        dto.Budget.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasApproved,
            IsWarn = budgets.Count > 0 && !hasApproved,
            Label  = hasApproved ? "Budget approvato presente" : "Nessun budget approvato",
            Detail = budgets.Count > 0 && !hasApproved
                ? "Approva il budget per procedere con la generazione delle rate."
                : null,
        });
        if (activeFyId.HasValue)
        {
            dto.Budget.Checks.Add(new SetupCheckDto
            {
                IsOk   = hasApprovedForActive,
                IsWarn = !hasApprovedForActive,
                Label  = hasApprovedForActive
                    ? $"Budget approvato presente per l'esercizio attivo «{activeYear!.Code}»"
                    : $"Nessun budget approvato per l'esercizio attivo «{activeYear!.Code}»",
                Detail = !hasApprovedForActive
                    ? "Crea e approva un budget preventivo per l'esercizio attivo."
                    : null,
            });
        }
        dto.Budget.Status = budgets.Count == 0     ? SetupSectionStatus.Error
            : !hasApproved                          ? SetupSectionStatus.Warn
            : !hasApprovedForActive                 ? SetupSectionStatus.Warn
            : SetupSectionStatus.Ok;

        return dto;
    }
}
