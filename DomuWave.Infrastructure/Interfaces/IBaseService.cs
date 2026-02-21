using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces
{
    /// <summary>
    /// Base interface for all services providing common CRUD operations
    /// </summary>
    public interface IBaseService<T, TKey> where T : class
    {
        Task<T> GetByIdAsync(TKey id, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<T>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken);
        Task<IList<T>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate, IUser currentUser, CancellationToken cancellationToken);
        Task<T> CreateAsync(T entity, IUser currentUser, CancellationToken cancellationToken);
        Task<T> UpdateAsync(T entity, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(TKey id, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> HardDeleteAsync(TKey id, IUser currentUser, CancellationToken cancellationToken);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(TKey id, IUser currentUser, CancellationToken cancellationToken);
        Task<(IList<T> Items, int TotalCount)> GetPagedAsync(Expression<Func<T, bool>> filter,
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>> orderBy,
            bool ascending,
            IUser currentUser,
            CancellationToken cancellationToken);
    }
}
