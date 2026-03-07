using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IMaintenanceService : IBaseService<Maintenance, int>
{
    Task<IList<Maintenance>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Maintenance>> GetByStatusAsync(int condominiumId, string status, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Maintenance>> GetByPriorityAsync(int condominiumId, string priority, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Maintenance>> GetOpenAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
}
