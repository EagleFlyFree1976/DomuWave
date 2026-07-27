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
    public class CondominiumFeeService : BaseService , ICondominiumFeeService
    {
            public CondominiumFeeService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "CondominiumFees";

        public async Task<CondominiumFee> GetByIdAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<CondominiumFee>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumFee>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumFee>> FindAsync(Expression<Func<CondominiumFee, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<CondominiumFee> CreateAsync(CondominiumFee entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<CondominiumFee> UpdateAsync(CondominiumFee entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var fee = await session.Query<CondominiumFee>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (fee == null)
                return false;

            fee.Trace(currentUser);
            fee.IsDeleted = true;
            await session.SaveOrUpdateAsync(fee, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var fee = await session.Query<CondominiumFee>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (fee == null)
                return false;

            await session.DeleteAsync(fee, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<CondominiumFee, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<CondominiumFee> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<CondominiumFee, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<CondominiumFee, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<CondominiumFee>()
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

        public async Task<IList<CondominiumFee>> GetByInstallmentIdAsync(int installmentId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => x.Installment.Id == installmentId && !x.IsDeleted && !x.Installment.IsDeleted)
                .OrderBy(x => x.Unit.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumFee>> GetByUnitIdAsync(int unitId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => x.Unit.Id == unitId && x.Tenant.Id == tenantId && !x.IsDeleted && !x.Installment.IsDeleted)
                .OrderByDescending(x => x.Installment.FiscalYear.StartDate.Year)
                .ThenByDescending(x => x.Installment.InstallmentNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumFee>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.Installment.IsDeleted)
                .OrderByDescending(x => x.Installment.FiscalYear.StartDate.Year)
                .ThenByDescending(x => x.Installment.InstallmentNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumFee>> GetUnpaidFeesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => x.Installment.Condominium.Id == condominiumId
                    && x.PaymentStatus != "Paid"
                    && !x.IsDeleted
                    && !x.Installment.IsDeleted)
                .OrderByDescending(x => x.Installment.FiscalYear.StartDate.Year)
                .ThenByDescending(x => x.Installment.InstallmentNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumFee>> GetOverdueFeesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            return await session.Query<CondominiumFee>()
                .Where(x => x.Installment.Condominium.Id == condominiumId
                    && x.Installment.DueDate < today
                    && x.PaymentStatus != "Paid"
                    && !x.IsDeleted
                    && !x.Installment.IsDeleted)
                .OrderByDescending(x => x.Installment.FiscalYear.StartDate.Year)
                .ThenByDescending(x => x.Installment.InstallmentNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalDueAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => x.UserId == userId
                    && x.PaymentStatus != "Paid"
                    && !x.IsDeleted
                    && !x.Installment.IsDeleted)
                .SumAsync(x => x.Balance, cancellationToken);
        }

        public async Task<decimal> GetTotalBalanceAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumFee>()
                .Where(x => x.UserId == userId && !x.IsDeleted && !x.Installment.IsDeleted)
                .SumAsync(x => x.Balance, cancellationToken);
        }

        public async Task<bool> RecordPaymentAsync(long feeId, decimal amount, DateTime paymentDate, string paymentMethod, long userId,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var fee = await session.Query<CondominiumFee>()
                .FirstOrDefaultAsync(x => x.Id == feeId && !x.IsDeleted, cancellationToken);

            if (fee == null)
                return false;

            // Aggiorna l'importo pagato e il saldo
            fee.AmountPaid += amount;
            fee.Balance = fee.AmountDue - fee.AmountPaid;

            // Se il saldo � <= 0, marca come pagato
            if (fee.Balance <= 0)
            {
                fee.PaymentStatus = "Paid";
                fee.Balance = 0;
            }
            else
            {
                fee.PaymentStatus = "PartiallyPaid";
            }

            fee.PaymentDate = paymentDate;
            fee.PaymentMethod = paymentMethod;
            fee.Trace(currentUser);

            await session.SaveOrUpdateAsync(fee, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }
    }
}
