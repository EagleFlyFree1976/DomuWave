using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class CreateCondominiumFeeCommand : BaseCommand, IQuery<Models.CondominiumFee>
{
    public Models.CondominiumFee Entity { get; set; }

    public CreateCondominiumFeeCommand() { }

    public CreateCondominiumFeeCommand(int currentUserId) : base(currentUserId) { }
    public CreateCondominiumFeeCommand(int currentUserId, Models.CondominiumFee entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
