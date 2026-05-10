using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IStaircaseService : IBaseService<Staircase, int>
{
    Task<IList<Staircase>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
}
