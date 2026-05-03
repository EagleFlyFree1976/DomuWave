using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IExtraordinaryWorkService : IBaseService<ExtraordinaryWork, int>
{
    Task<IList<ExtraordinaryWork>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<ExtraordinaryWork>> GetByStatusAsync(int condominiumId, string status, IUser currentUser, CancellationToken cancellationToken);
}
