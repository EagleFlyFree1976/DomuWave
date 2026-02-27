using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class CreateCondominiumCommand : BaseCommand, IQuery<Models.Condominium>
{
    public Models.Condominium Entity { get; set; }

    public CreateCondominiumCommand() { }

    public CreateCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public CreateCondominiumCommand(int currentUserId, Models.Condominium entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
