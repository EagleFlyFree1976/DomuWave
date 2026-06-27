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

        // NOTA: la generazione delle ExpenseAllocation (millesimale standard, criterio
        // misto art. 1124, conti a consumo, imputazione diretta) avviene in
        // ExpenseAllocationHelper.RegenerateAllocationsAsync, invocato dai consumer di
        // creazione/aggiornamento/rigenerazione spese. Questo service espone solo query.

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

    }
}
