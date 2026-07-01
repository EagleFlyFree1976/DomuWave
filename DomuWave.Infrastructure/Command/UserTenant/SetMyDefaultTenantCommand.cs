using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

/// <summary>
/// Imposta come predefinito (per l'utente corrente) il tenant indicato.
/// Usato dal selettore in sidebar per scegliere il contesto iniziale al login.
/// </summary>
public class SetMyDefaultTenantCommand : BaseCommand, IQuery<bool>
{
    public Guid TenantId { get; set; }

    public SetMyDefaultTenantCommand() { }

    public SetMyDefaultTenantCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
