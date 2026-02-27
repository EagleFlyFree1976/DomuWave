using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class DeleteCondominiumInstallmentCommand : BaseCommand, IQuery<bool>
{
    public int InstallmentId { get; set; }

    public DeleteCondominiumInstallmentCommand() { }

    public DeleteCondominiumInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public DeleteCondominiumInstallmentCommand(int currentUserId, int installmentId) : base(currentUserId)
    {
        InstallmentId = installmentId;
    }
}
