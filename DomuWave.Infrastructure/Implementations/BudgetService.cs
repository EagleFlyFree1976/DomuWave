using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using NHibernate.Linq;

using DomuWave.Services.Models;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class BudgetService : BaseService, IBudgetService
    {
        public BudgetService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Budgets";

        public async Task<Budget> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Budget>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Budget>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Budget>> FindAsync(Expression<Func<Budget, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Budget> CreateAsync(Budget entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Budget> UpdateAsync(Budget entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var budget = await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (budget == null)
                return false;

            budget.Trace(currentUser);
            budget.IsDeleted = true;
            await session.SaveOrUpdateAsync(budget, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var budget = await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (budget == null)
                return false;

            await session.DeleteAsync(budget, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Budget, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Budget> Items, int TotalCount)> GetPagedAsync(Expression<Func<Budget, bool>> filter,
            int pageNumber, int pageSize, Expression<Func<Budget, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Budget>()
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

        public async Task<IList<Budget>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<Budget> GetByYearAndTypeAsync(int condominiumId, int year, string type, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Condominium.Id == condominiumId 
                    && x.FiscalYear.StartDate.Year == year 
                    && x.Type == type 
                    && !x.IsDeleted);
        }

        public async Task<IList<Budget>> GetByYearAsync(int condominiumId, int year, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Budget>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.FiscalYear.StartDate.Year == year 
                    && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<Budget> GetCurrentBudgetAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            var currentYear = DateTime.Now.Year;
            return await session.Query<Budget>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.FiscalYear.StartDate.Year == currentYear 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ApproveBudgetAsync(int budgetId, long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            var budget = await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == budgetId);

            if (budget == null)
                return false;

            budget.Status = "Approved";
            budget.ApprovalDate = DateTime.Now;
            await session.SaveOrUpdateAsync(budget);
            await session.FlushAsync();
            return true;
        }

        public async Task<bool> CloseBudgetAsync(int budgetId, long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            var budget = await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == budgetId);

            if (budget == null)
                return false;

            budget.Status = "Closed";
            await session.SaveOrUpdateAsync(budget);
            await session.FlushAsync();
            return true;
        }
    }
}
