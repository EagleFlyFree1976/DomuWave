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
                var gross   = expense.GrossAmount;
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
                        AllocatedAmount = expense.GrossAmount,
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
        var grossAmount       = expense.GrossAmount;

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
        // Carica TUTTI i record esistenti (inclusi IsDeleted) per evitare violazioni
        // dell'indice univoco (ExpenseId, UnitId)
        var existing = await session.Query<ExpenseAllocation>()
            .Where(a => a.Expense.Id == expense.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingByUnit = existing.ToDictionary(a => a.Unit.Id);

        // Soft-delete unità non più presenti
        var activeUnitIds = rows.Select(r => r.UnitId).ToHashSet();
        foreach (var old in existing.Where(a => !a.IsDeleted && !activeUnitIds.Contains(a.Unit.Id)))
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
