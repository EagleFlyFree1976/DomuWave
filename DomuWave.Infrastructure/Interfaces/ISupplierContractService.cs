using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ISupplierContractService : IBaseService<SupplierContract, int>
    {
        Task<IList<SupplierContract>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<SupplierContract>> GetBySupplierIdAsync(int supplierId);
        Task<IList<SupplierContract>> GetActiveContractsAsync(int condominiumId);
        Task<IList<SupplierContract>> GetExpiringContractsAsync(int condominiumId, int daysAhead);
    }
}
