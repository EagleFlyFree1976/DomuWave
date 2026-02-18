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

using DomuWave.Domain.Models;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;
using NHibernate.Criterion;

namespace DomuWave.Services.Implementations
{
    public class ChartOfAccountsService : BaseService, IChartOfAccountsService
    {
                public ChartOfAccountsService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "ChartOfAccounts";

        public async Task<ChartOfAccounts> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<ChartOfAccounts>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<ChartOfAccounts>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<ChartOfAccounts>> FindAsync(Expression<Func<ChartOfAccounts, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChartOfAccounts> CreateAsync(ChartOfAccounts entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<ChartOfAccounts> UpdateAsync(ChartOfAccounts entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var chartOfAccount = await session.Query<ChartOfAccounts>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (chartOfAccount == null)
                return false;

            chartOfAccount.Trace(currentUser);
            chartOfAccount.IsDeleted = true;
            await session.SaveOrUpdateAsync(chartOfAccount, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var chartOfAccount = await session.Query<ChartOfAccounts>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (chartOfAccount == null)
                return false;

            await session.DeleteAsync(chartOfAccount, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<ChartOfAccounts, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<ChartOfAccounts> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<ChartOfAccounts, bool>> filter, Expression<Func<ChartOfAccounts, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<ChartOfAccounts>()
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

        public async Task<IList<ChartOfAccounts>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChartOfAccounts> GetByCodeAsync(int condominiumId, string code, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .FirstOrDefaultAsync(x => x.Condominium.Id == condominiumId 
                    && x.Code == code 
                    && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<ChartOfAccounts>> GetByTypeAsync(int condominiumId, string type, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.Type == type 
                    && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<ChartOfAccounts>> GetRootAccountsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.ParentAccount == null 
                    && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<ChartOfAccounts>> GetChildAccountsAsync(int parentAccountId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<ChartOfAccounts>()
                .Where(x => x.ParentAccount.Id == parentAccountId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }
    }
}
