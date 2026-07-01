using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesByInstallmentCommand : BaseCommand, IQuery<IList<CondominiumFeeReadDto>>
{
    public int InstallmentId { get; set; }
    public Guid TenantId { get; set; }

    public GetFeesByInstallmentCommand() { }

    public GetFeesByInstallmentCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesByInstallmentCommand(int currentUserId, int installmentId, Guid tenantId) : base(currentUserId)
    {
        InstallmentId = installmentId;
        TenantId      = tenantId;
    }
}
