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
        if (expense.MillesimalTable == null)
            return;

        var millesimalTableId = expense.MillesimalTable.Id;
        var grossAmount       = expense.GrossAmount;

        // 1. Carica le quote millesimali della tabella
        var unitMillesimals = await session.Query<UnitMillesimal>()
            .Where(um => um.MillesimalTable.Id == millesimalTableId && !um.IsDeleted)
            .Select(um => new
            {
                UnitId       = um.Unit.Id,
                UnitName     = um.Unit.DisplayName ?? um.Unit.InternalNumber,
                um.Millesimal,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!unitMillesimals.Any())
            return;

        decimal totalMillesimal = unitMillesimals.Sum(um => um.Millesimal);
        if (totalMillesimal == 0)
            return;

        // 2. Soft-cancella le allocazioni esistenti
        var existing = await session.Query<ExpenseAllocation>()
            .Where(a => a.Expense.Id == expense.Id && !a.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var old in existing)
        {
            old.IsDeleted = true;
            old.Trace(currentUser);
            await session.SaveOrUpdateAsync(old, cancellationToken).ConfigureAwait(false);
        }

        // 3. Prima passata — arrotonda a 2 decimali
        var rows = unitMillesimals
            .Select(um =>
            {
                decimal raw     = grossAmount * um.Millesimal / totalMillesimal;
                decimal rounded = Math.Round(raw, 2, MidpointRounding.AwayFromZero);
                return new { um.UnitId, um.Millesimal, Rounded = rounded };
            })
            .ToList();

        // 4. Assegna il residuo di arrotondamento all'unità con millesimo maggiore
        decimal roundedSum = rows.Sum(r => r.Rounded);
        decimal remainder  = Math.Round(grossAmount - roundedSum, 2, MidpointRounding.AwayFromZero);
        int?    largestId  = rows.Count > 0
            ? rows.OrderByDescending(r => r.Millesimal).First().UnitId
            : null;

        // 5. Crea i nuovi record
        foreach (var row in rows)
        {
            decimal rounding       = row.UnitId == largestId ? remainder : 0m;
            decimal allocatedAmount = row.Rounded + rounding;
            decimal pct             = totalMillesimal > 0 ? row.Millesimal / totalMillesimal * 100m : 0m;

            var unit       = session.Load<RealEstateUnit>(row.UnitId);
            var allocation = new ExpenseAllocation
            {
                Expense              = expense,
                Unit                 = unit,
                Tenant               = expense.Tenant,
                Millesimal           = row.Millesimal,
                AllocatedAmount      = allocatedAmount,
                AllocationPercentage = pct,
                RoundingAdjustment   = rounding,
                Notes                = "Generato automaticamente dalla tabella millesimale",
            };
            allocation.Trace(currentUser);
            await session.SaveOrUpdateAsync(allocation, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
