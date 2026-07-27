using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBilancioRipartizioneReportCommandConsumer
    : InMemoryConsumerBase<GetBilancioRipartizioneReportCommand, BilancioRipartizioneReportDto>
{
    private readonly IUserService _userService;

    public GetBilancioRipartizioneReportCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<BilancioRipartizioneReportDto> Consume(
        GetBilancioRipartizioneReportCommand command,
        IMediationContext                    mediationContext,
        CancellationToken                    ct)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var fy = await session.Query<Models.FiscalYear>()
            .FirstOrDefaultAsync(f => f.Id == command.FiscalYearId && !f.IsDeleted, ct)
            .ConfigureAwait(false);
        if (fy == null) throw new NotFoundException("Esercizio fiscale non trovato.");

        var condId = fy.Condominium?.Id ?? 0;

        // ── Unità attive del condominio ────────────────────────────────────────
        var units = await session.Query<RealEstateUnit>()
            .Where(u => u.Condominium.Id == condId && u.IsActive && !u.IsDeleted)
            .Select(u => new { u.Id, u.InternalNumber, u.DisplayName })
            .ToListAsync(ct).ConfigureAwait(false);

        // Proprietari e inquilini per unità (per i nominativi e per sapere se ci sono inquilini)
        var owners = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Condominium.Id == condId && o.IsActive && !o.IsDeleted)
            .Select(o => new { UnitId = o.Unit.Id, o.LastName, o.FirstName })
            .ToListAsync(ct).ConfigureAwait(false);

        var tenants = await session.Query<UnitTenant>()
            .Where(t => t.Unit.Condominium.Id == condId && t.IsActive && !t.IsDeleted)
            .Select(t => new { UnitId = t.Unit.Id, t.LastName, t.FirstName })
            .ToListAsync(ct).ConfigureAwait(false);

        var ownersByUnit  = owners.GroupBy(o => o.UnitId)
            .ToDictionary(g => g.Key, g => string.Join("-", g.Select(x => Surname(x.LastName, x.FirstName))));
        var tenantsByUnit = tenants.GroupBy(t => t.UnitId)
            .ToDictionary(g => g.Key, g => string.Join("-", g.Select(x => Surname(x.LastName, x.FirstName))));

        // ── Tipi di consumo del condominio + quote per unità (charge approvate) ──
        var consumptionTypes = await session.Query<ConsumptionType>()
            .Where(t => t.Condominium.Id == condId && !t.IsDeleted)
            .Select(t => new { t.Id, t.Name, t.UnitOfMeasure, AccountId = (int?)(t.Account != null ? t.Account.Id : 0) })
            .ToListAsync(ct).ConfigureAwait(false);

        var consumoRows = await session.Query<ConsumptionChargeItem>()
            .Where(ci => !ci.IsDeleted
                      && !ci.Charge.IsDeleted
                      && ci.Charge.Status.Id     == ConsumptionChargeStatus.Approved
                      && ci.Charge.FiscalYear.Id == fy.Id)
            .Select(ci => new
            {
                TypeId   = ci.Charge.ConsumptionType.Id,
                UnitId   = ci.Unit.Id,
                ci.Consumption,
                ci.Amount,
            })
            .ToListAsync(ct).ConfigureAwait(false);

        // Mappa: (tipoId, unitId) -> (quantità, importo)
        var consumoByTypeUnit = consumoRows
            .GroupBy(x => (x.TypeId, x.UnitId))
            .ToDictionary(g => g.Key, g => (Qta: g.Sum(x => x.Consumption), Imp: g.Sum(x => x.Amount)));

        // ChargeabilityType del conto di ciascun tipo consumo (per assegnare P/I)
        var consumoTypeAccountIds = consumptionTypes
            .Where(t => t.AccountId.HasValue && t.AccountId.Value > 0)
            .Select(t => t.AccountId!.Value).Distinct().ToList();

        var accountChargeability = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == condId && !a.IsDeleted)
            .Select(a => new { a.Id, ChargeId = (int)(a.ChargeabilityType.Id), TypeId = (int)a.Type })
            .ToListAsync(ct).ConfigureAwait(false);
        var accChargeMap = accountChargeability.ToDictionary(a => a.Id, a => a.ChargeId);
        var accTypeMap   = accountChargeability.ToDictionary(a => a.Id, a => a.TypeId);

        // Conti a consumo (vanno nelle colonne consumi, esclusi dalle colonne millesimali)
        var consumoAccountIds = consumoTypeAccountIds.ToHashSet();

        // ── Spese dell'esercizio + allocazioni ──────────────────────────────────
        var expenses = await session.Query<Expense>()
            .Where(e => e.FiscalYear.Id == fy.Id && !e.IsDeleted)
            .Select(e => new
            {
                ExpenseId        = e.Id,
                AccountId        = e.Account != null ? e.Account.Id : 0,
                AccountType      = e.Account != null ? (int)e.Account.Type : 0,
                ChargeabilityId  = e.ChargeabilityType != null ? e.ChargeabilityType.Id : ChargeabilityType.Owner,
                MillesimalTableId = (int?)(e.MillesimalTable != null ? e.MillesimalTable.Id : 0),
                MillesimalTableName = e.MillesimalTable != null ? e.MillesimalTable.Name : null,
                DirectUnitId     = (int?)(e.Unit != null ? e.Unit.Id : 0),
            })
            .ToListAsync(ct).ConfigureAwait(false);

        var expenseById = expenses.ToDictionary(e => e.ExpenseId);
        var expenseIds  = expenses.Select(e => e.ExpenseId).ToList();

        var allocations = expenseIds.Any()
            ? await session.Query<ExpenseAllocation>()
                .Where(a => expenseIds.Contains(a.Expense.Id) && !a.IsDeleted && a.AllocatedAmount != 0)
                .Select(a => new { ExpenseId = a.Expense.Id, UnitId = a.Unit.Id, a.AllocatedAmount })
                .ToListAsync(ct).ConfigureAwait(false)
            : new();

        // ── Versato per unità = quote condominiali incassate (CondominiumFee) ────
        // NB: filtrare tramite l'Installment (non tramite il Budget) perché le rate
        // manuali hanno Budget == null: passando per Budget si escluderebbero le loro quote.
        var versatoRows = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.FiscalYear.Id == fy.Id
                     && f.Installment.Condominium.Id == condId
                     && !f.IsDeleted
                     && !f.Installment.IsDeleted)
            .Select(f => new { UnitId = f.Unit.Id, f.AmountPaid })
            .ToListAsync(ct).ConfigureAwait(false);
        var versatoByUnit = versatoRows
            .GroupBy(x => x.UnitId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.AmountPaid));

        // ── Override manuali delle celle del bilancio ──────────────────────────
        // Sostituiscono il valore calcolato per la cella (unità, riga, tipo, colonna).
        var overrideMap = (await session.Query<BilancioRipartizioneOverride>()
                .Where(o => o.FiscalYear.Id == fy.Id && !o.IsDeleted)
                .Select(o => new { o.Unit.Id, o.RowType, o.CellType, o.ColumnRefId, o.Amount })
                .ToListAsync(ct).ConfigureAwait(false))
            .GroupBy(o => (o.Id, o.RowType, o.CellType, o.ColumnRefId))
            .ToDictionary(g => g.Key, g => g.Last().Amount);

        // Modificabile solo se l'esercizio non è Closed/Locked.
        bool isEditable = fy.Status?.Id != FiscalYearStatus.Closed
                       && fy.Status?.Id != FiscalYearStatus.Locked;

        // Cerca l'override per una cella; ritorna true e l'importo se presente.
        bool TryOverride(int unitId, int rowType, int cellType, int columnRefId, out decimal amount)
            => overrideMap.TryGetValue((unitId, rowType, cellType, columnRefId), out amount);

        // ── Determina colonne USATE ─────────────────────────────────────────────
        var usedConsumoTypeIds = consumoByTypeUnit.Keys.Select(k => k.TypeId).ToHashSet();
        var consumoColumns = consumptionTypes
            .Where(t => usedConsumoTypeIds.Contains(t.Id))
            .Select(t => new ConsumoColumnDto { ConsumptionTypeId = t.Id, Name = t.Name ?? "Consumo", UnitOfMeasure = t.UnitOfMeasure })
            .ToList();

        // Tabelle millesimali usate (da spese NON dirette e NON su conti a consumo)
        var usedTablesRaw = expenses
            .Where(e => e.MillesimalTableId.HasValue && e.MillesimalTableId.Value > 0
                     && !(e.DirectUnitId.HasValue && e.DirectUnitId.Value > 0)
                     && !consumoAccountIds.Contains(e.AccountId))
            .GroupBy(e => e.MillesimalTableId!.Value)
            .Select(g => new MillesimaleColumnDto { MillesimalTableId = g.Key, Name = g.First().MillesimalTableName ?? "Tabella" })
            .ToList();

        // Determina quali tabelle usate sono "di default" per ordinarle per prime.
        var usedTableIdsAll = usedTablesRaw.Select(t => t.MillesimalTableId).ToList();
        var defaultTableIds = usedTableIdsAll.Any()
            ? (await session.Query<MillesimalTable>()
                .Where(t => usedTableIdsAll.Contains(t.Id) && t.IsDefault && !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync(ct).ConfigureAwait(false))
                .ToHashSet()
            : new HashSet<int>();

        foreach (var t in usedTablesRaw)
            t.IsDefault = defaultTableIds.Contains(t.MillesimalTableId);

        // Ordine: tabella di default per prima, poi le altre in ordine alfabetico.
        var usedTables = usedTablesRaw
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .ToList();

        // Millesimi per (tabella, unità) per le tabelle usate
        var usedTableIds = usedTables.Select(t => t.MillesimalTableId).ToList();
        var millesimalByTableUnit = usedTableIds.Any()
            ? (await session.Query<UnitMillesimal>()
                .Where(um => usedTableIds.Contains(um.MillesimalTable.Id) && !um.IsDeleted)
                .Select(um => new { TableId = um.MillesimalTable.Id, UnitId = um.Unit.Id, um.Millesimal })
                .ToListAsync(ct).ConfigureAwait(false))
                .ToDictionary(x => (x.TableId, x.UnitId), x => x.Millesimal)
            : new Dictionary<(int, int), decimal>();

        // ── Costruzione righe ───────────────────────────────────────────────────
        var report = new BilancioRipartizioneReportDto
        {
            FiscalYearId    = fy.Id,
            FiscalYearCode  = fy.Code,
            CondominiumId   = condId,
            CondominiumName = fy.Condominium?.Name,
            ConsumoColumns     = consumoColumns,
            MillesimaleColumns = usedTables,
        };

        // Ordine alfabetico per nominativo proprietario (A→Z); fallback al nome unità
        // per le unità senza proprietario, tie-breaker su InternalNumber.
        string OwnerKey(int unitId, string? displayName, string? internalNumber) =>
            ownersByUnit.TryGetValue(unitId, out var on) && !string.IsNullOrWhiteSpace(on)
                ? on
                : FallbackUnitName(displayName, internalNumber);

        var orderedUnits = units
            .OrderBy(x => OwnerKey(x.Id, x.DisplayName, x.InternalNumber), StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.InternalNumber, StringComparer.CurrentCultureIgnoreCase);

        foreach (var u in orderedUnits)
        {
            bool hasTenant = tenantsByUnit.ContainsKey(u.Id);

            // Un importo va sulla riga Inquilini solo se è 'a carico Inquilino' E l'unità
            // ha effettivamente un inquilino; altrimenti ricade sul Proprietario.
            // (Owner e Auto → sempre Proprietari.)
            bool GoesToTenant(int chargeId) => chargeId == ChargeabilityType.Tenant && hasTenant;

            // Costruisci la riga per un dato "destinatario" (proprietari/inquilini)
            BilancioRowDto BuildRow(bool tenantRow)
            {
                int rowType = tenantRow ? BilancioRowType.Inquilini : BilancioRowType.Proprietari;

                var row = new BilancioRowDto
                {
                    UnitId     = u.Id,
                    Nominativo = tenantRow
                        ? (tenantsByUnit.TryGetValue(u.Id, out var tn) ? tn : "")
                        : (ownersByUnit.TryGetValue(u.Id, out var on) ? on : FallbackUnitName(u.DisplayName, u.InternalNumber)),
                    RowType    = tenantRow ? "Inquilini" : "Proprietari",
                };

                // Totale "Totale speso" calcolato (senza override), per lo scostamento.
                decimal totaleCalcolato = 0m;

                // Consumi → riga in base al ChargeabilityType del conto del tipo consumo
                foreach (var col in consumoColumns)
                {
                    var typeAcc = consumptionTypes.First(t => t.Id == col.ConsumptionTypeId).AccountId ?? 0;
                    var chargeId = accChargeMap.TryGetValue(typeAcc, out var cid) ? cid : ChargeabilityType.Owner;
                    bool toTenant = GoesToTenant(chargeId);

                    decimal qta = 0m, imp = 0m;
                    if (toTenant == tenantRow &&
                        consumoByTypeUnit.TryGetValue((col.ConsumptionTypeId, u.Id), out var cv))
                    {
                        qta = cv.Qta; imp = cv.Imp;
                    }
                    totaleCalcolato += imp;
                    bool ov = TryOverride(u.Id, rowType, BilancioCellType.Consumo, col.ConsumptionTypeId, out var ovImp);
                    if (ov) imp = ovImp;
                    row.Consumi.Add(new ConsumoCellDto { ConsumptionTypeId = col.ConsumptionTypeId, Quantita = qta, Importo = imp, IsOverridden = ov });
                }

                decimal accrediti = 0m;

                // Millesimali → solo USCITE ripartite per tabella, assegnate per ChargeabilityType.
                // Le ENTRATE ripartite (es. GSE distribuito) confluiscono negli Accrediti.
                foreach (var col in usedTables)
                {
                    decimal imp = 0m;
                    foreach (var a in allocations.Where(a => a.UnitId == u.Id))
                    {
                        var e = expenseById[a.ExpenseId];
                        if (e.MillesimalTableId != col.MillesimalTableId) continue;
                        if (e.DirectUnitId.HasValue && e.DirectUnitId.Value > 0) continue;
                        if (consumoAccountIds.Contains(e.AccountId)) continue;
                        bool toTenant = GoesToTenant(e.ChargeabilityId);
                        if (toTenant != tenantRow) continue;
                        if (e.AccountType == (int)ChartOfAccountsType.Entrata) { accrediti += a.AllocatedAmount; continue; }
                        imp += a.AllocatedAmount;
                    }
                    totaleCalcolato += imp;
                    var mill = (!tenantRow && millesimalByTableUnit.TryGetValue((col.MillesimalTableId, u.Id), out var mv)) ? mv : 0m;
                    bool ovM = TryOverride(u.Id, rowType, BilancioCellType.Millesimale, col.MillesimalTableId, out var ovImpM);
                    if (ovM) imp = ovImpM;
                    row.Millesimali.Add(new MillesimaleCellDto { MillesimalTableId = col.MillesimalTableId, Millesimal = mill, Importo = imp, IsOverridden = ovM });
                }

                // Entrate ripartite (millesimali) su conti a consumo o tabelle non in colonna → comunque accrediti
                foreach (var a in allocations.Where(a => a.UnitId == u.Id))
                {
                    var e = expenseById[a.ExpenseId];
                    if (e.DirectUnitId.HasValue && e.DirectUnitId.Value > 0) continue;
                    if (e.AccountType != (int)ChartOfAccountsType.Entrata) continue;
                    if (GoesToTenant(e.ChargeabilityId) != tenantRow) continue;
                    bool inColonna = e.MillesimalTableId.HasValue
                                  && usedTables.Any(t => t.MillesimalTableId == e.MillesimalTableId.Value)
                                  && !consumoAccountIds.Contains(e.AccountId);
                    if (inColonna) continue; // già conteggiata nel loop sopra
                    accrediti += a.AllocatedAmount;
                }

                // Dirette (uscite imputate direttamente all'unità) e Accrediti diretti (entrate dirette)
                decimal dirette = 0m;
                foreach (var e in expenses.Where(e => e.DirectUnitId == u.Id))
                {
                    bool toTenant = GoesToTenant(e.ChargeabilityId);
                    if (toTenant != tenantRow) continue;
                    var amt = allocations.Where(a => a.ExpenseId == e.ExpenseId && a.UnitId == u.Id)
                                         .Sum(a => a.AllocatedAmount);
                    if (e.AccountType == (int)ChartOfAccountsType.Entrata) accrediti += amt;
                    else                                                   dirette   += amt;
                }
                // Dirette/Accrediti calcolati confluiscono nel totale calcolato.
                totaleCalcolato += dirette - accrediti;

                // Override su Dirette / Accrediti (ColumnRefId = 0).
                bool ovD = TryOverride(u.Id, rowType, BilancioCellType.Dirette, 0, out var ovDir);
                if (ovD) dirette = ovDir;
                bool ovA = TryOverride(u.Id, rowType, BilancioCellType.Accrediti, 0, out var ovAcc);
                if (ovA) accrediti = ovAcc;

                row.Dirette            = dirette;
                row.DiretteOverridden  = ovD;
                row.Accrediti          = accrediti;
                row.AccreditiOverridden = ovA;

                row.Totale = row.Consumi.Sum(c => c.Importo)
                           + row.Millesimali.Sum(m => m.Importo)
                           + row.Dirette
                           - row.Accrediti;

                // Versato (quote incassate) attribuito alla riga proprietari per non duplicare
                decimal versatoCalc = (!tenantRow && versatoByUnit.TryGetValue(u.Id, out var v)) ? v : 0m;
                bool ovV = TryOverride(u.Id, rowType, BilancioCellType.Versato, 0, out var ovVer);
                row.Versato          = ovV ? ovVer : versatoCalc;
                row.VersatoOverridden = ovV;
                row.Saldo   = row.Versato - row.Totale;

                // Accumula i totali "Totale speso" calcolato/effettivo per lo scostamento.
                report.TotaleCalcolato += totaleCalcolato;
                report.TotaleEffettivo += row.Totale;
                if (row.Consumi.Any(c => c.IsOverridden) || row.Millesimali.Any(m => m.IsOverridden)
                    || ovD || ovA || ovV)
                    report.HasOverrides = true;
                return row;
            }

            // Riga proprietari sempre; riga inquilini solo se presenti
            report.Rows.Add(BuildRow(tenantRow: false));
            if (hasTenant) report.Rows.Add(BuildRow(tenantRow: true));
        }

        // ── Totali di colonna ───────────────────────────────────────────────────
        report.Totals.Consumi = consumoColumns.Select(col => new ConsumoCellDto
        {
            ConsumptionTypeId = col.ConsumptionTypeId,
            Quantita = report.Rows.SelectMany(r => r.Consumi).Where(c => c.ConsumptionTypeId == col.ConsumptionTypeId).Sum(c => c.Quantita),
            Importo  = report.Rows.SelectMany(r => r.Consumi).Where(c => c.ConsumptionTypeId == col.ConsumptionTypeId).Sum(c => c.Importo),
        }).ToList();
        report.Totals.Millesimali = usedTables.Select(col => new MillesimaleCellDto
        {
            MillesimalTableId = col.MillesimalTableId,
            Millesimal = report.Rows.SelectMany(r => r.Millesimali).Where(m => m.MillesimalTableId == col.MillesimalTableId).Sum(m => m.Millesimal),
            Importo    = report.Rows.SelectMany(r => r.Millesimali).Where(m => m.MillesimalTableId == col.MillesimalTableId).Sum(m => m.Importo),
        }).ToList();
        report.Totals.Dirette   = report.Rows.Sum(r => r.Dirette);
        report.Totals.Accrediti = report.Rows.Sum(r => r.Accrediti);
        report.Totals.Totale    = report.Rows.Sum(r => r.Totale);
        report.Totals.Versato   = report.Rows.Sum(r => r.Versato);
        report.Totals.Saldo     = report.Rows.Sum(r => r.Saldo);

        report.IsEditable = isEditable;

        return report;
    }

    private static string Surname(string? lastName, string? firstName)
    {
        var ln = (lastName ?? "").Trim();
        if (ln.Length > 0) return ln;
        return (firstName ?? "").Trim();
    }

    private static string FallbackUnitName(string? displayName, string? internalNumber)
        => !string.IsNullOrWhiteSpace(displayName) ? displayName!
         : !string.IsNullOrWhiteSpace(internalNumber) ? internalNumber!
         : "—";
}
