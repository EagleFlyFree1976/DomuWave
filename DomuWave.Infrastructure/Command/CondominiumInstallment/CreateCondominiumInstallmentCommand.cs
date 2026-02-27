using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class CreateCondominiumInstallmentCommand : BaseCommand, IQuery<Models.CondominiumInstallment>
{
    public Models.CondominiumInstallment Entity { get; set; }

    public CreateCondominiumInstallmentCommand() { }

    public CreateCondominiumInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public CreateCondominiumInstallmentCommand(int currentUserId, Models.CondominiumInstallment entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
