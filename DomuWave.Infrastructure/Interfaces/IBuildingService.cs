using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IBuildingService : IBaseService<Building, int>
{
    Task<IList<Building>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
}
