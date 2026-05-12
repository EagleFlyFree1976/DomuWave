using CPQ.Core.Memberships;
using CPQ.Core.Services;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IAssemblyService : IBaseService<Assembly, int>
{
    Task<Assembly?> GetByIdAsync(int id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Assembly>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
    Task<Assembly?>       GetWithDetailsAsync(int id, IUser currentUser, CancellationToken cancellationToken);
}
