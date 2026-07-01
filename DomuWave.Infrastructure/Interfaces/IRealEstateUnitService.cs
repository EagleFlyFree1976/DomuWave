using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IRealEstateUnitService : IBaseService<RealEstateUnit, int>
    {
        Task<RealEstateUnit> GetByIdAsync(int id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<RealEstateUnit>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<RealEstateUnit>> GetByStaircaseAsync(int condominiumId, string staircase, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<RealEstateUnit>> GetByFloorAsync(int condominiumId, int floor, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<RealEstateUnit>> GetByTypeAsync(int condominiumId, string unitType, IUser currentUser, CancellationToken cancellationToken);
        Task<int> GetTotalUnitsCountAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);

        /// <summary>
        /// Restituisce gli Id delle unità di cui l'utente Condomino è proprietario o
        /// inquilino (in base a UnitOwner/UnitTenant.UserId). Usato per limitare la
        /// visibilità del condòmino alle sole proprie unità.
        /// </summary>
        Task<IList<int>> GetCondominoUnitIdsAsync(long userId, CancellationToken cancellationToken);
    }
}
