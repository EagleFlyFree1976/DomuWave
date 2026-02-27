using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class UpdateCondominiumInstallmentCommand : BaseCommand, IQuery<Models.CondominiumInstallment>
{
    public int InstallmentId { get; set; }
    public Models.CondominiumInstallment Entity { get; set; }

    public UpdateCondominiumInstallmentCommand() { }

    public UpdateCondominiumInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public UpdateCondominiumInstallmentCommand(int currentUserId, int installmentId, Models.CondominiumInstallment entity) : base(currentUserId)
    {
        InstallmentId = installmentId;
        Entity = entity;
    }
}
