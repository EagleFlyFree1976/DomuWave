using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ISupplierService : IBaseService<Supplier, int>
    {
        Task<IList<Supplier>> GetByTypeAsync(Guid tenantId, string supplierType);
        Task<Supplier> GetByVatNumberAsync(Guid tenantId, string vatNumber);
        Task<IList<Supplier>> SearchSuppliersAsync(Guid tenantId, string searchTerm);
    }
}
