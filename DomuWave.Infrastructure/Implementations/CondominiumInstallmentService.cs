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
    public class CondominiumInstallmentService : BaseService, ICondominiumInstallmentService
    {
            public CondominiumInstallmentService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "CondominiumInstallments";

        public async Task<CondominiumInstallment> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<CondominiumInstallment>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumInstallment>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumInstallment>> FindAsync(Expression<Func<CondominiumInstallment, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<CondominiumInstallment> CreateAsync(CondominiumInstallment entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<CondominiumInstallment> UpdateAsync(CondominiumInstallment entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var installment = await session.Query<CondominiumInstallment>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (installment == null)
                return false;

            installment.Trace(currentUser);
            installment.IsDeleted = true;

            foreach (var fee in installment.Fees.Where(f => !f.IsDeleted))
            {
                fee.Trace(currentUser);
                fee.IsDeleted = true;
            }

            await session.SaveOrUpdateAsync(installment, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var installment = await session.Query<CondominiumInstallment>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (installment == null)
                return false;

            await session.DeleteAsync(installment, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<CondominiumInstallment, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<CondominiumInstallment> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<CondominiumInstallment, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<CondominiumInstallment, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<CondominiumInstallment>()
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

        public async Task<IList<CondominiumInstallment>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .Where(x => x.Condominium.Id == condominiumId && x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumInstallment>> GetByFiscalYearIdAsync(int condominiumId, int fiscalYearId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .Where(x => x.Condominium.Id == condominiumId
                    && x.FiscalYear.Id == fiscalYearId
                    && !x.IsDeleted)
                .OrderBy(x => x.InstallmentNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<CondominiumInstallment> GetByYearAndNumberAsync(int condominiumId, int year, int installmentNumber, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .FirstOrDefaultAsync(x => x.Condominium.Id == condominiumId 
                    && x.FiscalYear.StartDate.Year == year 
                    && x.InstallmentNumber == installmentNumber 
                    && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<CondominiumInstallment>> GetOpenInstallmentsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumInstallment>()
                .Where(x => x.Condominium.Id == condominiumId
                    && x.Status.Id != CondominiumInstallmentStatus.Paid
                    && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumInstallment>> GetOverdueInstallmentsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            return await session.Query<CondominiumInstallment>()
                .Where(x => x.Condominium.Id == condominiumId
                    && x.DueDate < today
                    && x.Status.Id != CondominiumInstallmentStatus.Paid
                    && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> GenerateInstallmentsAsync(int condominiumId, int fiscalYearId, int budgetId, long userId, IUser currentUser,
            CancellationToken cancellationToken)
        {
            var condominium = await session.Query<Condominium>()
                .FirstOrDefaultAsync(x => x.Id == condominiumId, cancellationToken);

            if (condominium == null)
                return false;

            var fiscalYear = await session.Query<FiscalYear>()
                .FirstOrDefaultAsync(x => x.Id == fiscalYearId && x.Condominium.Id == condominiumId && !x.IsDeleted, cancellationToken);

            if (fiscalYear == null)
                return false;

            var budget = await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == budgetId && x.Condominium.Id == condominiumId, cancellationToken);

            if (budget == null)
                return false;

            // Verifica se le rate sono già state generate per questo esercizio
            var existingInstallments = await session.Query<CondominiumInstallment>()
                .Where(x => x.Condominium.Id == condominiumId && x.FiscalYear.Id == fiscalYearId && !x.IsDeleted)
                .CountAsync(cancellationToken);

            if (existingInstallments > 0)
                return false;

            var openStatus = await session.Query<CondominiumInstallmentStatus>()
                .FirstOrDefaultAsync(x => x.Id == CondominiumInstallmentStatus.Open, cancellationToken);

            // Genera 4 rate equamente distribuite nell'esercizio fiscale
            var totalDays   = (fiscalYear.EndDate - fiscalYear.StartDate).Days;
            var installments = new List<CondominiumInstallment>();
            var amountPerInstallment = budget.TotalIncome / 4;

            for (int i = 1; i <= 4; i++)
            {
                var dueDate = fiscalYear.StartDate.AddDays((int)(totalDays * i / 4.0));

                var installment = new CondominiumInstallment
                {
                    Condominium       = condominium,
                    Budget            = budget,
                    FiscalYear        = fiscalYear,
                    InstallmentNumber = i,
                    DueDate           = dueDate,
                    TotalAmount       = amountPerInstallment,
                    Status            = openStatus,
                    Notes             = $"Rata {i} – esercizio {fiscalYear.Code}",
                };

                installment.Trace(currentUser);
                installments.Add(installment);
            }

            foreach (var installment in installments)
            {
                await session.SaveOrUpdateAsync(installment, cancellationToken);
            }

            await session.FlushAsync(cancellationToken);
            return true;
        }
    }
}
