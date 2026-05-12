using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IStaircaseService : IBaseService<Staircase, int>
{
    Task<Staircase> GetByIdAsync(int id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Staircase>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
}
