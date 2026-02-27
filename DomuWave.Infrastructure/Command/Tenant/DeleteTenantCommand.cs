using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

/// <summary>
/// Soft-delete di un Tenant. Restituisce true se eliminato, false se non trovato.
/// </summary>
public class DeleteTenantCommand : BaseCommand, IQuery<bool>
{
    public Guid TenantId { get; set; }

    public DeleteTenantCommand() { }

    public DeleteTenantCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
