using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IRealEstateUnitService : IBaseService<RealEstateUnit, int>
    {
        Task<IList<RealEstateUnit>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<RealEstateUnit>> GetByStaircaseAsync(int condominiumId, string staircase);
        Task<IList<RealEstateUnit>> GetByFloorAsync(int condominiumId, int floor);
        Task<IList<RealEstateUnit>> GetByTypeAsync(int condominiumId, string unitType);
        Task<int> GetTotalUnitsCountAsync(int condominiumId);
    }
}
