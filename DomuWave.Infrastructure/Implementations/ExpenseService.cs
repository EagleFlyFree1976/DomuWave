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
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class ExpenseService : BaseService, IExpenseService
    {
        public ExpenseService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion { get; }

        public async Task<Expense> GetByIdAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Expense>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Expense>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Expense>> FindAsync(Expression<Func<Expense, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => !x.IsDeleted)
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Expense> CreateAsync(Expense entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Expense> UpdateAsync(Expense entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (expense == null)
                return false;

            expense.Trace(currentUser);
            await session.DeleteAsync(expense, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (expense == null)
                return false;

            await session.DeleteAsync(expense, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Expense, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Expense> Items, int TotalCount)> GetPagedAsync(Expression<Func<Expense, bool>> filter,
            int pageNumber, int pageSize, Expression<Func<Expense, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Expense>().Where(x => !x.IsDeleted).Where(filter);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = ascending
                ? await query
                    .OrderBy(orderBy)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken)
                : await query
                    .OrderByDescending(orderBy)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<IList<Expense>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Expense>> GetByDateRangeAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted && x.PaymentDate >= startDate && x.PaymentDate <= endDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Expense>> GetBySupplierIdAsync(int supplierId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => x.Supplier.Id == supplierId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Expense>> GetByTypeAsync(int condominiumId, string expenseType, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => x.Condominium.Id == condominiumId && x.ExpenseType == expenseType && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Expense>> GetUnpaidExpensesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => x.Condominium.Id == condominiumId && !x.PaymentDate.HasValue && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalExpensesAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<Expense>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted && x.PaymentDate >= startDate && x.PaymentDate <= endDate)
                .SumAsync(x => x.GrossAmount, cancellationToken);
        }

        public async Task<bool> MarkAsPaidAsync(long expenseId, DateTime paymentDate, string paymentMethod,
            IUser currentUser,
            CancellationToken cancellationToken)
        {
            var expense = await session.Query<Expense>()
                .FirstOrDefaultAsync(x => x.Id == expenseId && !x.IsDeleted, cancellationToken);

            if (expense == null)
                return false;

            expense.PaymentDate = paymentDate;
            expense.PaymentMethod = paymentMethod;
            expense.Trace(currentUser);

            await session.SaveOrUpdateAsync(expense, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }
    }
}
