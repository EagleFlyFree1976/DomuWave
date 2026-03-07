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
            .Where(a => a.Condominium.Id == condId && !a.IsDeleted)
            .Select(a => new { a.Id, a.IsActive })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var activeAccounts = accounts.Count(a => a.IsActive);

        dto.ChartOfAccounts.Checks.Add(new SetupCheckDto
        {
            IsOk   = accounts.Count > 0,
            Label  = $"{accounts.Count} conti configurati",
            Detail = accounts.Count == 0 ? "Aggiungi i conti o copia da un altro condominio." : null,
        });
        dto.ChartOfAccounts.Checks.Add(new SetupCheckDto
        {
            IsOk   = activeAccounts > 0,
            IsWarn = accounts.Count > 0 && activeAccounts == 0,
            Label  = $"{activeAccounts} conti attivi",
        });
        dto.ChartOfAccounts.Status = accounts.Count == 0 ? SetupSectionStatus.Error
            : activeAccounts == 0                         ? SetupSectionStatus.Warn
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
        var fiscalYears = await session.Query<FiscalYear>()
            .Where(fy => fy.Condominium.Id == condId && !fy.IsDeleted)
            .Select(fy => new { fy.Id, fy.Status })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var hasOpen = fiscalYears.Any(fy => fy.Status.Id == FiscalYearStatus.Open);

        dto.FiscalYear.Checks.Add(new SetupCheckDto
        {
            IsOk   = fiscalYears.Count > 0,
            Label  = $"{fiscalYears.Count} esercizi fiscali",
            Detail = fiscalYears.Count == 0 ? "Crea un esercizio fiscale." : null,
        });
        dto.FiscalYear.Checks.Add(new SetupCheckDto
        {
            IsOk   = hasOpen,
            IsWarn = fiscalYears.Count > 0 && !hasOpen,
            Label  = hasOpen ? "Esercizio aperto attivo" : "Nessun esercizio aperto",
            Detail = !hasOpen ? "Apri o crea un esercizio fiscale per l'anno corrente." : null,
        });
        dto.FiscalYear.Status = fiscalYears.Count == 0 ? SetupSectionStatus.Error
            : !hasOpen                                  ? SetupSectionStatus.Warn
            : SetupSectionStatus.Ok;

        // ── 6. Budget ─────────────────────────────────────────────────────────
        var budgets = await session.Query<Budget>()
            .Where(b => b.Condominium.Id == condId && !b.IsDeleted)
            .Select(b => new { b.Id, b.Status })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var hasApproved = budgets.Any(b => b.Status.Id == BudgetStatus.Approved);

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
        dto.Budget.Status = budgets.Count == 0 ? SetupSectionStatus.Error
            : !hasApproved                      ? SetupSectionStatus.Warn
            : SetupSectionStatus.Ok;

        return dto;
    }
}
