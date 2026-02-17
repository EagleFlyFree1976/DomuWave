using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IUnitOwnerService : IBaseService<UnitOwner, int>
    {
        Task<IList<UnitOwner>> GetByUnitIdAsync(int unitId);
        Task<IList<UnitOwner>> GetByUserIdAsync(long userId);
        Task<IList<UnitOwner>> GetActiveOwnersAsync(int unitId);
        Task<decimal> GetTotalOwnershipQuotaAsync(int unitId);
    }
}
