using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesByInstallmentCommand : BaseCommand, IQuery<IList<CondominiumFeeReadDto>>
{
    public int InstallmentId { get; set; }

    public GetFeesByInstallmentCommand() { }

    public GetFeesByInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesByInstallmentCommand(int currentUserId, int installmentId) : base(currentUserId)
    {
        InstallmentId = installmentId;
    }
}
