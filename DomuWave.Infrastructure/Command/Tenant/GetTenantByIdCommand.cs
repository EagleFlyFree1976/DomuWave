using DomuWave.Services.Dto.Tenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

/// <summary>
/// Recupera un singolo tenant per ID (Guid).
/// </summary>
public class GetTenantByIdCommand : BaseCommand, IQuery<TenantReadDto>
{
    /// <summary>ID del tenant da recuperare.</summary>
    public Guid TenantId { get; set; }

    public GetTenantByIdCommand() { }

    public GetTenantByIdCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
