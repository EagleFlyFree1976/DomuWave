using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesByInstallmentCommand : BaseCommand, IQuery<IList<Models.CondominiumFee>>
{
    public int InstallmentId { get; set; }

    public GetFeesByInstallmentCommand() { }

    public GetFeesByInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesByInstallmentCommand(int currentUserId, int installmentId) : base(currentUserId)
    {
        InstallmentId = installmentId;
    }
}
