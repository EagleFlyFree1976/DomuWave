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
                UnitId      = um.Unit.Id,
                um.Millesimal,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!unitMillesimals.Any())
            return;

        decimal totalMillesimal = unitMillesimals.Sum(um => um.Millesimal);
        if (totalMillesimal == 0)
            return;

        // 2. Carica TUTTI i record esistenti (inclusi IsDeleted) per evitare violazioni
        //    dell'indice univoco (ExpenseId, UnitId) durante l'upsert.
        var existing = await session.Query<ExpenseAllocation>()
            .Where(a => a.Expense.Id == expense.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingByUnit = existing.ToDictionary(a => a.Unit.Id);

        // Unità non più nella tabella millesimale → soft-delete
        var activeUnitIds = unitMillesimals.Select(um => um.UnitId).ToHashSet();
        foreach (var old in existing.Where(a => !a.IsDeleted && !activeUnitIds.Contains(a.Unit.Id)))
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

        // 5. Upsert: aggiorna il record esistente (ri-attivandolo se necessario) o ne crea uno nuovo
        foreach (var row in rows)
        {
            decimal rounding        = row.UnitId == largestId ? remainder : 0m;
            decimal allocatedAmount = row.Rounded + rounding;
            decimal pct             = totalMillesimal > 0 ? row.Millesimal / totalMillesimal * 100m : 0m;

            if (existingByUnit.TryGetValue(row.UnitId, out var alloc))
            {
                alloc.IsDeleted           = false;
                alloc.Millesimal          = row.Millesimal;
                alloc.AllocatedAmount     = allocatedAmount;
                alloc.AllocationPercentage = pct;
                alloc.RoundingAdjustment  = rounding;
                alloc.Notes               = "Generato automaticamente dalla tabella millesimale";
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
                    AllocatedAmount      = allocatedAmount,
                    AllocationPercentage = pct,
                    RoundingAdjustment   = rounding,
                    Notes                = "Generato automaticamente dalla tabella millesimale",
                };
                newAlloc.Trace(currentUser);
                await session.SaveOrUpdateAsync(newAlloc, cancellationToken).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
