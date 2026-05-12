using DomuWave.Services.Dto.Condominium;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class GetCondominiumByIdCommand : BaseTenantRelatedCommand, IQuery<CondominiumReadDto>
{
    public int CondominiumId { get; set; }

    public GetCondominiumByIdCommand() { }

    public GetCondominiumByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetCondominiumByIdCommand(int currentUserId, Guid tenantId, int condominiumId) : base(currentUserId, tenantId)
    {
        CondominiumId = condominiumId;
    }
}
