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
    public class SupplierContractService : BaseService, ISupplierContractService
    {
        public SupplierContractService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "SupplierContracts";

        public async Task<SupplierContract> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<SupplierContract> GetByIdAsync(int id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .FirstOrDefaultAsync(x => x.Id == id && x.Tenant.Id == tenantId && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> FindAsync(Expression<Func<SupplierContract, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<SupplierContract> CreateAsync(SupplierContract entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<SupplierContract> UpdateAsync(SupplierContract entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var contract = await session.Query<SupplierContract>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (contract == null)
                return false;

            contract.Trace(currentUser);
            contract.IsDeleted = true;
            await session.SaveOrUpdateAsync(contract, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var contract = await session.Query<SupplierContract>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (contract == null)
                return false;

            await session.DeleteAsync(contract, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<SupplierContract, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<SupplierContract> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<SupplierContract, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<SupplierContract, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<SupplierContract>()
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

        public async Task<IList<SupplierContract>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .Where(x => x.Condominium.Id == condominiumId && x.Tenant.Id == tenantId && !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetBySupplierIdAsync(int supplierId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<SupplierContract>()
                .Where(x => x.Supplier.Id == supplierId && !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetActiveContractsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            return await session.Query<SupplierContract>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.StartDate <= today 
                    && (x.EndDate == null || x.EndDate >= today) 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetExpiringContractsAsync(int condominiumId, int daysAhead, IUser currentUser,
            CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            var targetDate = today.AddDays(daysAhead);

            return await session.Query<SupplierContract>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.EndDate.HasValue
                    && x.EndDate >= today
                    && x.EndDate <= targetDate 
                    && !x.IsDeleted)
                .OrderBy(x => x.EndDate)
                .ToListAsync(cancellationToken);
        }
    }
}
