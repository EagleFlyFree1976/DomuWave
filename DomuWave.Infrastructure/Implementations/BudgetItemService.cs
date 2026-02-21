using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Domain.Models;
using NHibernate.Linq;

using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class BudgetItemService : BaseService, IBudgetItemService
    {
        public BudgetItemService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "BudgetItems";
                
        public async Task<BudgetItem> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IList<BudgetItem>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<BudgetItem>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .Where(x => x.Tenant.Id == tenantId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<BudgetItem>> FindAsync(Expression<Func<BudgetItem, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<BudgetItem> CreateAsync(BudgetItem entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<BudgetItem> UpdateAsync(BudgetItem entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            
            var item = await session.Query<BudgetItem>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (item == null)
                return false;

            item.Trace(currentUser);
            item.IsDeleted = true;
            await session.SaveOrUpdateAsync(item, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            
            var item = await session.Query<BudgetItem>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (item == null)
                return false;

            await session.DeleteAsync(item, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<BudgetItem, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>().AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<(IList<BudgetItem> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<BudgetItem, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<BudgetItem, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            
            var query = session.Query<BudgetItem>().Where(filter);

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

        public async Task<IList<BudgetItem>> GetByBudgetIdAsync(int budgetId, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .Where(x => x.Budget.Id == budgetId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<BudgetItem>> GetByAccountIdAsync(int accountId, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .Where(x => x.Account.Id == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalAmountAsync(int budgetId, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<BudgetItem>()
                .Where(x => x.Budget.Id == budgetId && !x.IsDeleted)
                .SumAsync(x => x.Amount, cancellationToken);
        }
    }
}
