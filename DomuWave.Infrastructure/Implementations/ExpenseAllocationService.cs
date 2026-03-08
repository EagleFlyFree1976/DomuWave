using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Models;
using NHibernate.Linq;

using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class ExpenseAllocationService : BaseService, IExpenseAllocationService
    {
        public ExpenseAllocationService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "ExpenseAllocations";

        public async Task<ExpenseAllocation> GetByIdAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<ExpenseAllocation>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<ExpenseAllocation>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<ExpenseAllocation>> FindAsync(Expression<Func<ExpenseAllocation, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<ExpenseAllocation> CreateAsync(ExpenseAllocation entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<ExpenseAllocation> UpdateAsync(ExpenseAllocation entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var expenseAllocation = await session.Query<ExpenseAllocation>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (expenseAllocation == null)
                return false;

            expenseAllocation.Trace(currentUser);
            expenseAllocation.IsDeleted = true;
            await session.SaveOrUpdateAsync(expenseAllocation, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var expenseAllocation = await session.Query<ExpenseAllocation>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (expenseAllocation == null)
                return false;

            await session.DeleteAsync(expenseAllocation, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<ExpenseAllocation, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<ExpenseAllocation> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<ExpenseAllocation, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<ExpenseAllocation, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<ExpenseAllocation>()
                .Where(x => !x.IsDeleted)
                .Where(filter);

            var totalCount = await query.CountAsync(cancellationToken);

            if (ascending)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(orderBy);
            }

            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var items = await query.ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<IList<ExpenseAllocation>> GetByExpenseIdAsync(long expenseId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .Where(x => x.Expense.Id == expenseId && !x.IsDeleted)
                .OrderBy(x => x.Unit.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<ExpenseAllocation>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            IQueryable<ExpenseAllocation> query = session.Query<ExpenseAllocation>();
            query = query.Where(x => x.Unit.Id == unitId && !x.IsDeleted);
            

            return await query.OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalAllocationAsync(long expenseId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ExpenseAllocation>()
                .Where(x => x.Expense.Id == expenseId && !x.IsDeleted)
                .SumAsync(x => x.AllocatedAmount, cancellationToken);
        }

        public async Task<bool> AllocateExpenseAsync(long expenseId, long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            var expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == expenseId, cancellationToken);

            if (expense == null)
                return false;

            var millesimalTable = expense.MillesimalTable;
            if (millesimalTable == null)
                return false;

            var units = await session.Query<RealEstateUnit>()
                .Where(x => x.Condominium.Id == expense.Condominium.Id && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!units.Any())
                return false;

            var account = expense.Account;
            var allocationMethod = account?.AllocationMethod ?? AllocationMethod.Standard;

            // Mappa unit -> millesimalEntry per evitare doppi loop
            var millesimalMap = units.ToDictionary(
                u => u.Id,
                u => millesimalTable.Expenses.SelectMany(l => l.Allocations)
                         .FirstOrDefault(x => x.Unit.Id == u.Id && !x.IsDeleted));

            decimal totalMillesimal = millesimalMap.Values
                .Where(e => e != null)
                .Sum(e => e.Millesimal);

            if (totalMillesimal == 0)
                return false;

            var allocations = new List<ExpenseAllocation>();

            if (allocationMethod == AllocationMethod.Mixed
                && account.MillesimalPercentage.HasValue
                && account.FloorWeight.HasValue
                && account.InhabitantsWeight.HasValue)
            {
                decimal millesimalPct = account.MillesimalPercentage.Value / 100m;
                decimal factorPct     = 1m - millesimalPct;
                decimal floorW        = account.FloorWeight.Value;
                decimal inhabW        = account.InhabitantsWeight.Value;

                decimal millesimalPart = expense.NetAmount * millesimalPct;
                decimal factorPart     = expense.NetAmount * factorPct;

                // Calcola il fattore per ogni unità (floor 0 trattato come 1)
                var factorMap = units.ToDictionary(
                    u => u.Id,
                    u => floorW * Math.Max(u.Floor, 1) + inhabW * Math.Max(u.NumeroAbitanti, 0));

                decimal totalFactor = factorMap.Values.Sum();

                foreach (var unit in units)
                {
                    if (!millesimalMap.TryGetValue(unit.Id, out var millesimalEntry) || millesimalEntry == null)
                        continue;

                    decimal millesimalShare = totalMillesimal > 0
                        ? millesimalPart * millesimalEntry.Millesimal / totalMillesimal
                        : 0m;

                    decimal factorShare = totalFactor > 0
                        ? factorPart * factorMap[unit.Id] / totalFactor
                        : 0m;

                    decimal allocatedAmount    = millesimalShare + factorShare;
                    decimal allocationPct      = expense.NetAmount > 0 ? allocatedAmount / expense.NetAmount * 100m : 0m;

                    var allocation = new ExpenseAllocation
                    {
                        Expense              = expense,
                        Unit                 = unit,
                        Millesimal           = millesimalEntry.Millesimal,
                        AllocatedAmount      = allocatedAmount,
                        AllocationPercentage = allocationPct,
                        Notes                = $"Auto-allocated: {account.MillesimalPercentage}% millesimale + {100 - account.MillesimalPercentage}% fattore piano/abitanti",
                    };
                    allocation.Trace(currentUser);
                    allocations.Add(allocation);
                }
            }
            else
            {
                // Standard: 100% millesimale
                foreach (var unit in units)
                {
                    if (!millesimalMap.TryGetValue(unit.Id, out var millesimalEntry) || millesimalEntry == null)
                        continue;

                    decimal allocationPercentage = (millesimalEntry.Millesimal / totalMillesimal) * 100;
                    decimal allocatedAmount       = (expense.NetAmount * millesimalEntry.Millesimal) / totalMillesimal;

                    var allocation = new ExpenseAllocation
                    {
                        Expense              = expense,
                        Unit                 = unit,
                        Millesimal           = millesimalEntry.Millesimal,
                        AllocatedAmount      = allocatedAmount,
                        AllocationPercentage = allocationPercentage,
                        Notes                = "Auto-allocated based on millesimal table",
                    };
                    allocation.Trace(currentUser);
                    allocations.Add(allocation);
                }
            }

            foreach (var allocation in allocations)
            {
                await session.SaveOrUpdateAsync(allocation, cancellationToken);
            }

            await session.FlushAsync(cancellationToken);
            return true;
        }
    }
}
