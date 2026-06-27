using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using NHibernate;
using NHibernate.Linq;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Genera/rigenera i record ExpenseAllocation per una spesa a partire dalla sua tabella millesimale.
/// Soft-cancella le allocazioni precedenti e ne crea di nuove.
/// </summary>
internal static class ExpenseAllocationHelper
{
    public static async Task DeleteAllocationsAsync(
        ISession          session,
        long              expenseId,
        IUser             currentUser,
        CancellationToken cancellationToken)
    {
        var existing = await session.Query<ExpenseAllocation>()
            .Where(a => a.Expense.Id == expenseId && !a.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var old in existing)
        {
            old.IsDeleted = true;
            old.Trace(currentUser);
            await session.SaveOrUpdateAsync(old, cancellationToken).ConfigureAwait(false);
        }

        if (existing.Any())
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Importo della spesa a carico dei condòmini, da ripartire alle unità.
    /// In DomuWave GrossAmount è già al netto della ritenuta d'acconto
    /// (imponibile + IVA + cassa + bollo − ritenuta), ma la ritenuta resta un costo
    /// a carico del condominio (versata all'Erario per conto del fornitore): va quindi
    /// reintegrata. Coincide con il totale fattura usato dal report spese per millesimale.
    /// </summary>
    private static decimal ChargeableAmount(Expense expense)
        => expense.GrossAmount + expense.WithholdingTax;

    public static async Task RegenerateAllocationsAsync(
        ISession          session,
        Expense           expense,
        IUser             currentUser,
        CancellationToken cancellationToken)
    {
        // ── Conti "a consumo" ──────────────────────────────────────────────────
        // Se il conto della spesa è associato a un tipo di consumo, la bolletta NON
        // va MAI ripartita a millesimi:
        //   - se esiste una ripartizione consumi APPROVATA → quote per consumo
        //   - altrimenti → nessuna allocazione (in attesa dell'approvazione)
        if (expense.Account != null && expense.FiscalYear != null)
        {
            var isConsumptionAccount = await session.Query<ConsumptionType>()
                .AnyAsync(t => !t.IsDeleted
                            && t.Account != null
                            && t.Account.Id == expense.Account.Id
                            && t.Condominium.Id == expense.Condominium.Id,
                          cancellationToken)
                .ConfigureAwait(false);

            if (isConsumptionAccount)
            {
                var consumptionPerc = await session.Query<ConsumptionChargeItem>()
                    .Where(ci => !ci.IsDeleted
                              && !ci.Charge.IsDeleted
                              && ci.Charge.Status.Id == ConsumptionChargeStatus.Approved
                              && ci.Charge.FiscalYear.Id == expense.FiscalYear.Id
                              && ci.Charge.ConsumptionType.Account.Id == expense.Account.Id)
                    .Select(ci => new { UnitId = ci.Unit.Id, ci.Percentage })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!consumptionPerc.Any())
                {
                    // Conto a consumo ma ripartizione non ancora approvata:
                    // azzera eventuali allocazioni (niente ripartizione a millesimi).
                    await DeleteAllocationsAsync(session, expense.Id, currentUser, cancellationToken)
                        .ConfigureAwait(false);
                    return;
                }

                // Ripartisci l'importo di QUESTA bolletta secondo le percentuali di consumo,
                // assegnando il residuo da arrotondamento all'unità con percentuale maggiore.
                var gross   = ChargeableAmount(expense);
                var ordered = consumptionPerc.OrderByDescending(p => p.Percentage).ToList();
                decimal allocatedSum = 0m;
                var rows = new List<AllocationRow>();
                for (int i = 0; i < ordered.Count; i++)
                {
                    var p       = ordered[i];
                    decimal amount = Math.Round(gross * p.Percentage, 2, MidpointRounding.AwayFromZero);
                    rows.Add(new AllocationRow
                    {
                        UnitId          = p.UnitId,
                        Millesimal      = 0m,
                        AllocatedAmount = amount,
                        Percentage      = p.Percentage * 100m,
                        Notes           = "Ripartito per consumi",
                    });
                    allocatedSum += amount;
                }
                // Aggiusta il residuo sull'unità con percentuale maggiore
                var consumptionRemainder = Math.Round(gross - allocatedSum, 2, MidpointRounding.AwayFromZero);
                if (consumptionRemainder != 0m && rows.Count > 0)
                {
                    rows[0].AllocatedAmount += consumptionRemainder;
                    rows[0].Rounding         = consumptionRemainder;
                }

                await UpsertAllocationsAsync(session, expense, currentUser, cancellationToken, rows);
                return;
            }
        }

        // Fonte: imputazione a un singolo immobile — intera spesa sull'unità (100%)
        if (expense.Unit != null)
        {
            await UpsertAllocationsAsync(
                session, expense, currentUser, cancellationToken,
                rows: new List<AllocationRow>
                {
                    new AllocationRow
                    {
                        UnitId          = expense.Unit.Id,
                        Millesimal      = 0m,
                        AllocatedAmount = ChargeableAmount(expense),
                        Percentage      = 100m,
                        Notes           = "Imputazione diretta a singolo immobile",
                    }
                });
            return;
        }

        // Fonte: tabella millesimale standard
        if (expense.MillesimalTable == null)
            return;

        var millesimalTableId = expense.MillesimalTable.Id;
        var grossAmount       = ChargeableAmount(expense);

        var unitMillesimals = await session.Query<UnitMillesimal>()
            .Where(um => um.MillesimalTable.Id == millesimalTableId && !um.IsDeleted)
            .Select(um => new { UnitId = um.Unit.Id, um.Millesimal })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!unitMillesimals.Any())
            return;

        decimal totalMillesimal = unitMillesimals.Sum(um => um.Millesimal);
        if (totalMillesimal == 0)
            return;

        // ── Criterio misto valore/altezza (art. 1124 c.c.: scale e ascensori) ──────
        // Una parte (MillesimalPercentage%) si ripartisce per valore (tabella millesimale),
        // la restante parte per altezza/uso, in proporzione al fattore:
        //   FloorWeight * Piano  +  InhabitantsWeight * NumeroAbitanti
        // Il piano terra (Floor = 0) NON paga la quota per altezza, ma paga comunque la
        // quota per valore (millesimi) — conforme alla giurisprudenza costante.
        var account = expense.Account;
        if (account != null
            && account.AllocationMethod == AllocationMethod.Mixed
            && account.MillesimalPercentage.HasValue
            && account.FloorWeight.HasValue
            && account.InhabitantsWeight.HasValue)
        {
            var rows = await BuildMixedAllocationRowsAsync(
                session, expense, grossAmount, unitMillesimals.ToDictionary(um => um.UnitId, um => um.Millesimal),
                totalMillesimal, account, cancellationToken).ConfigureAwait(false);

            if (rows != null)
            {
                await UpsertAllocationsAsync(session, expense, currentUser, cancellationToken, rows);
                return;
            }
            // rows == null → fattore totale nullo (es. tutte unità al piano terra senza
            // abitanti): si ricade sulla ripartizione 100% millesimale qui sotto.
        }

        // Prima passata — arrotonda a 2 decimali
        var rawRows = unitMillesimals
            .Select(um =>
            {
                decimal raw     = grossAmount * um.Millesimal / totalMillesimal;
                decimal rounded = Math.Round(raw, 2, MidpointRounding.AwayFromZero);
                return new { um.UnitId, um.Millesimal, Rounded = rounded };
            })
            .ToList();

        // Residuo all'unità con millesimo maggiore
        decimal roundedSum = rawRows.Sum(r => r.Rounded);
        decimal remainder  = Math.Round(grossAmount - roundedSum, 2, MidpointRounding.AwayFromZero);
        int?    largestId  = rawRows.Count > 0
            ? rawRows.OrderByDescending(r => r.Millesimal).First().UnitId
            : null;

        await UpsertAllocationsAsync(
            session, expense, currentUser, cancellationToken,
            rows: rawRows.Select(r =>
            {
                decimal rounding = r.UnitId == largestId ? remainder : 0m;
                decimal amount   = r.Rounded + rounding;
                return new AllocationRow
                {
                    UnitId          = r.UnitId,
                    Millesimal      = r.Millesimal,
                    AllocatedAmount = amount,
                    Percentage      = totalMillesimal > 0 ? r.Millesimal / totalMillesimal * 100m : 0m,
                    Rounding        = rounding,
                    Notes           = "Generato automaticamente dalla tabella millesimale",
                };
            }).ToList());
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Costruisce le righe di ripartizione per il criterio misto valore/altezza
    /// (art. 1124 c.c.). Restituisce null se il fattore altezza/uso complessivo è 0
    /// (in quel caso il chiamante ricade sulla ripartizione 100% millesimale).
    /// </summary>
    private static async Task<List<AllocationRow>?> BuildMixedAllocationRowsAsync(
        ISession                 session,
        Expense                  expense,
        decimal                  grossAmount,
        IReadOnlyDictionary<int, decimal> millesimalByUnit,
        decimal                  totalMillesimal,
        ChartOfAccounts          account,
        CancellationToken        cancellationToken)
    {
        // Solo le unità presenti nella tabella millesimale partecipano alla ripartizione.
        var unitIds = millesimalByUnit.Keys.ToList();

        var unitData = await session.Query<RealEstateUnit>()
            .Where(u => unitIds.Contains(u.Id) && !u.IsDeleted)
            .Select(u => new { u.Id, u.Floor, u.NumeroAbitanti })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        decimal millesimalPct = account.MillesimalPercentage!.Value / 100m;   // quota per valore
        decimal factorPct     = 1m - millesimalPct;                            // quota per altezza/uso
        decimal floorW        = account.FloorWeight!.Value;
        decimal inhabW        = account.InhabitantsWeight!.Value;

        decimal millesimalPart = grossAmount * millesimalPct;
        decimal factorPart     = grossAmount * factorPct;

        // Fattore per unità: piano terra (Floor = 0) → quota altezza nulla (Math.Max(_, 0)).
        var factorByUnit = unitData.ToDictionary(
            u => u.Id,
            u => floorW * Math.Max(u.Floor, 0) + inhabW * Math.Max(u.NumeroAbitanti, 0));

        decimal totalFactor = factorByUnit.Values.Sum();
        if (totalFactor == 0m)
            return null;   // nessuna base per la quota altezza → fallback al 100% millesimale

        // Prima passata: importi grezzi arrotondati
        var raw = new Dictionary<int, (decimal rounded, decimal millesimal)>();
        foreach (var unitId in unitIds)
        {
            decimal millesimal = millesimalByUnit[unitId];

            decimal millesimalShare = totalMillesimal > 0
                ? millesimalPart * millesimal / totalMillesimal
                : 0m;
            decimal factorShare = factorByUnit.TryGetValue(unitId, out var f)
                ? factorPart * f / totalFactor
                : 0m;

            decimal rounded = Math.Round(millesimalShare + factorShare, 2, MidpointRounding.AwayFromZero);
            raw[unitId] = (rounded, millesimal);
        }

        // Residuo da arrotondamento all'unità con la quota millesimale maggiore
        decimal roundedSum = raw.Values.Sum(x => x.rounded);
        decimal remainder  = Math.Round(grossAmount - roundedSum, 2, MidpointRounding.AwayFromZero);
        int?    largestId  = raw.Count > 0
            ? raw.OrderByDescending(kv => kv.Value.millesimal).First().Key
            : null;

        int millesimalLabel = (int)account.MillesimalPercentage.Value;
        return raw.Select(kv =>
        {
            decimal rounding = kv.Key == largestId ? remainder : 0m;
            decimal amount   = kv.Value.rounded + rounding;
            return new AllocationRow
            {
                UnitId          = kv.Key,
                Millesimal      = kv.Value.millesimal,
                AllocatedAmount = amount,
                Percentage      = grossAmount > 0 ? amount / grossAmount * 100m : 0m,
                Rounding        = rounding,
                Notes           = $"Criterio misto: {millesimalLabel}% per valore (millesimi) + {100 - millesimalLabel}% per altezza/uso (art. 1124 c.c.)",
            };
        }).ToList();
    }

    private sealed class AllocationRow
    {
        public int     UnitId          { get; init; }
        public decimal Millesimal      { get; init; }
        public decimal AllocatedAmount { get; set; }
        public decimal Percentage      { get; init; }
        public decimal Rounding        { get; set; }
        public string? Notes           { get; init; }
    }

    private static async Task UpsertAllocationsAsync(
        ISession           session,
        Expense            expense,
        IUser              currentUser,
        CancellationToken  cancellationToken,
        List<AllocationRow> rows)
    {
        // Consolida eventuali righe duplicate per la stessa unità in un'unica allocazione.
        // Dati millesimali sporchi (più voci UnitMillesimal non eliminate per la stessa
        // (tabella, unità)) produrrebbero altrimenti due insert con la stessa coppia
        // (ExpenseId, UnitId) → violazione di IX_ExpenseAllocation_Unique.
        rows = rows
            .GroupBy(r => r.UnitId)
            .Select(g => new AllocationRow
            {
                UnitId          = g.Key,
                Millesimal      = g.Sum(x => x.Millesimal),
                AllocatedAmount = g.Sum(x => x.AllocatedAmount),
                Percentage      = g.Sum(x => x.Percentage),
                Rounding        = g.Sum(x => x.Rounding),
                Notes           = g.First().Notes,
            })
            .ToList();

        // Carica TUTTI i record esistenti (inclusi IsDeleted) per evitare violazioni
        // dell'indice univoco (ExpenseId, UnitId)
        var existing = await session.Query<ExpenseAllocation>()
            .Where(a => a.Expense.Id == expense.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Possono esistere più righe soft-deleted per la stessa unità (l'indice univoco
        // filtra IsDeleted = 0): per ogni unità tieni quella attiva, se c'è, altrimenti la
        // prima, e soft-cancella le altre per evitare future violazioni dell'indice.
        var existingByUnit = new Dictionary<int, ExpenseAllocation>();
        foreach (var grp in existing.GroupBy(a => a.Unit.Id))
        {
            var keep = grp.FirstOrDefault(a => !a.IsDeleted) ?? grp.First();
            existingByUnit[grp.Key] = keep;
            foreach (var dup in grp.Where(a => a != keep && !a.IsDeleted))
            {
                dup.IsDeleted = true;
                dup.Trace(currentUser);
                await session.SaveOrUpdateAsync(dup, cancellationToken).ConfigureAwait(false);
            }
        }

        // Soft-delete unità non più presenti
        var activeUnitIds = rows.Select(r => r.UnitId).ToHashSet();
        foreach (var old in existingByUnit.Values.Where(a => !a.IsDeleted && !activeUnitIds.Contains(a.Unit.Id)))
        {
            old.IsDeleted = true;
            old.Trace(currentUser);
            await session.SaveOrUpdateAsync(old, cancellationToken).ConfigureAwait(false);
        }

        // Upsert
        foreach (var row in rows)
        {
            if (existingByUnit.TryGetValue(row.UnitId, out var alloc))
            {
                alloc.IsDeleted            = false;
                alloc.Millesimal           = row.Millesimal;
                alloc.AllocatedAmount      = row.AllocatedAmount;
                alloc.AllocationPercentage = row.Percentage;
                alloc.RoundingAdjustment   = row.Rounding;
                alloc.Notes                = row.Notes;
                alloc.Trace(currentUser);
                await session.SaveOrUpdateAsync(alloc, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var unit = session.Load<RealEstateUnit>(row.UnitId);
                var newAlloc = new ExpenseAllocation
                {
                    Expense              = expense,
                    Unit                 = unit,
                    Tenant               = expense.Tenant,
                    Millesimal           = row.Millesimal,
                    AllocatedAmount      = row.AllocatedAmount,
                    AllocationPercentage = row.Percentage,
                    RoundingAdjustment   = row.Rounding,
                    Notes                = row.Notes,
                };
                newAlloc.Trace(currentUser);
                await session.SaveOrUpdateAsync(newAlloc, cancellationToken).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
