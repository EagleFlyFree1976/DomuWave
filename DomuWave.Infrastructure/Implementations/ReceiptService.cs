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

namespace DomuWave.Services.Implementations
{
    public class ReceiptService : BaseService, IReceiptService
    {
            public ReceiptService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Receipts";

        public async Task<Receipt> GetByIdAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Receipt>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Receipt>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Receipt>> FindAsync(Expression<Func<Receipt, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Receipt> CreateAsync(Receipt entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Receipt> UpdateAsync(Receipt entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var receipt = await session.Query<Receipt>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (receipt == null)
                return false;

            receipt.Trace(currentUser);
            receipt.IsDeleted = true;
            await session.SaveOrUpdateAsync(receipt, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var receipt = await session.Query<Receipt>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (receipt == null)
                return false;

            await session.DeleteAsync(receipt, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Receipt, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Receipt> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Receipt, bool>> filter, Expression<Func<Receipt, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Receipt>()
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

        public async Task<IList<Receipt>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .OrderByDescending(x => x.ReceiptDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Receipt>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.ReceiptDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Receipt>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => x.Unit.Id == unitId && !x.IsDeleted)
                .OrderByDescending(x => x.ReceiptDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Receipt>> GetByDateRangeAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.ReceiptDate >= startDate 
                    && x.ReceiptDate <= endDate 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.ReceiptDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Receipt>> GetUnreconciledReceiptsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && (x.ReconciliationStatus == null 
                        || x.ReconciliationStatus != "Reconciled") 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.ReceiptDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ReconcileReceiptAsync(long receiptId, long feeId, long userId, IUser currentUser,
            CancellationToken cancellationToken)
        {
            var receipt = await session.Query<Receipt>()
                .FirstOrDefaultAsync(x => x.Id == receiptId && !x.IsDeleted, cancellationToken);

            if (receipt == null)
                return false;

            var fee = await session.Query<CondominiumFee>()
                .FirstOrDefaultAsync(x => x.Id == feeId && !x.IsDeleted, cancellationToken);

            if (fee == null)
                return false;

            // Verifica che l'importo della ricevuta corrisponda al saldo della tassa
            if (receipt.Amount != fee.Balance)
                return false;

            // Aggiorna la ricevuta
            receipt.Fee = fee;
            receipt.ReconciliationStatus = "Reconciled";
            receipt.Trace(currentUser);

            // Aggiorna la tassa
            fee.AmountPaid += receipt.Amount;
            fee.Balance = fee.AmountDue - fee.AmountPaid;
            fee.PaymentStatus = "Paid";
            fee.PaymentDate = receipt.ReceiptDate;
            fee.Trace(currentUser);

            await session.SaveOrUpdateAsync(receipt, cancellationToken);
            await session.SaveOrUpdateAsync(fee, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<decimal> GetTotalReceiptsAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<Receipt>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.ReceiptDate >= startDate 
                    && x.ReceiptDate <= endDate 
                    && !x.IsDeleted)
                .SumAsync(x => x.Amount, cancellationToken);
        }
    }
}
