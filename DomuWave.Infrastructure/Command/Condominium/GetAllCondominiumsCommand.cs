using DomuWave.Services.Dto.Condominium;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class GetAllCondominiumsCommand : BaseCommand, IQuery<IList<CondominiumReadDto>>
{
    public Guid TenantId { get; set; }

    public GetAllCondominiumsCommand() { }

    public GetAllCondominiumsCommand(int currentUserId) : base(currentUserId) { }
    public GetAllCondominiumsCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
