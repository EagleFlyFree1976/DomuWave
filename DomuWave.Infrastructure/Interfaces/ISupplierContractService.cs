using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ISupplierContractService : IBaseService<SupplierContract, int>
    {
        Task<IList<SupplierContract>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<SupplierContract>> GetBySupplierIdAsync(int supplierId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<SupplierContract>> GetActiveContractsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<SupplierContract>> GetExpiringContractsAsync(int condominiumId, int daysAhead, IUser currentUser, CancellationToken cancellationToken);
    }
}
