using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class GetAllSuppliersCommand : BaseCommand, IQuery<IList<Models.Supplier>>
{
    public Guid TenantId { get; set; }

    public GetAllSuppliersCommand() { }

    public GetAllSuppliersCommand(int currentUserId) : base(currentUserId) { }
    public GetAllSuppliersCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
