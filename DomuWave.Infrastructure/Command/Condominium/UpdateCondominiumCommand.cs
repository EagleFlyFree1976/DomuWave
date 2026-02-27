using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class UpdateCondominiumCommand : BaseCommand, IQuery<Models.Condominium>
{
    public int CondominiumId { get; set; }
    public Models.Condominium Entity { get; set; }

    public UpdateCondominiumCommand() { }

    public UpdateCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public UpdateCondominiumCommand(int currentUserId, int condominiumId, Models.Condominium entity) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Entity = entity;
    }
}
