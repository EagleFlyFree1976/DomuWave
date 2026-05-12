using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ISupplierService : IBaseService<Supplier, int>
    {
        Task<Supplier> GetByIdAsync(int id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Supplier>> GetByTypeAsync(Guid tenantId, string supplierType, IUser currentUser, CancellationToken cancellationToken);
        Task<Supplier> GetByVatNumberAsync(Guid tenantId, string vatNumber, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Supplier>> SearchSuppliersAsync(Guid tenantId, string searchTerm, IUser currentUser, CancellationToken cancellationToken);
    }
}
