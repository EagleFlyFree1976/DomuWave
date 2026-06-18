using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IAdminTaskService : IBaseService<AdminTask, int>
{
    /// <summary>Task del tenant, con i condomìni collegati, filtrabili per assegnatario/stato/scadenza.</summary>
    Task<IList<AdminTask>> GetByTenantFilteredAsync(
        Guid tenantId, int? assignedToUserId, int? statusId, DateTime? dueBefore,
        IUser currentUser, CancellationToken cancellationToken);

    /// <summary>Singolo task con i condomìni collegati caricati.</summary>
    Task<AdminTask?> GetByIdWithCondominiumsAsync(int id, IUser currentUser, CancellationToken cancellationToken);
}
