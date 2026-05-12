using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class DeleteCondominiumCommand : BaseTenantRelatedCommand, IQuery<bool>
{
    public int CondominiumId { get; set; }

    public DeleteCondominiumCommand() { }

    public DeleteCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public DeleteCondominiumCommand(int currentUserId, Guid tenantId, int condominiumId) : base(currentUserId, tenantId)
    {
        CondominiumId = condominiumId;
    }
}
