using CPQ.Core.Memberships;
using CPQ.Core.Services;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IAssemblyService : IBaseService<Assembly, int>
{
    Task<IList<Assembly>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
    Task<Assembly?>       GetWithDetailsAsync(int id, IUser currentUser, CancellationToken cancellationToken);
}
