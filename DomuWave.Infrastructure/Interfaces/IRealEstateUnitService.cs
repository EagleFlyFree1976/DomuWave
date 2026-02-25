using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IRealEstateUnitService : IBaseService<RealEstateUnit, int>
    {
        Task<IList<RealEstateUnit>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<RealEstateUnit>> GetByStaircaseAsync(int condominiumId, string staircase, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<RealEstateUnit>> GetByFloorAsync(int condominiumId, int floor, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<RealEstateUnit>> GetByTypeAsync(int condominiumId, string unitType, IUser currentUser, CancellationToken cancellationToken);
        Task<int> GetTotalUnitsCountAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
    }
}
