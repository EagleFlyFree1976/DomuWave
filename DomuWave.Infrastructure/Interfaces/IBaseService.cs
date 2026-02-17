using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DomuWave.Services.Interfaces
{
    /// <summary>
    /// Base interface for all services providing common CRUD operations
    /// </summary>
    public interface IBaseService<T, TKey> where T : class
    {
        Task<T> GetByIdAsync(TKey id);
        Task<IList<T>> GetAllAsync();
        Task<IList<T>> GetByTenantIdAsync(Guid tenantId);
        Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(TKey id);
        Task<bool> HardDeleteAsync(TKey id);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        Task<bool> ExistsAsync(TKey id);
        Task<(IList<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true);
    }
}
