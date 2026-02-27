using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class GetActiveCondominiumsCommand : BaseCommand, IQuery<IList<Models.Condominium>>
{
    public Guid TenantId { get; set; }

    public GetActiveCondominiumsCommand() { }

    public GetActiveCondominiumsCommand(int currentUserId) : base(currentUserId) { }
    public GetActiveCondominiumsCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
