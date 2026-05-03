using DomuWave.Services.Dto.CondominiumInstallment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GetInstallmentsByCondominiumCommand : BaseCommand, IQuery<IList<CondominiumInstallmentReadDto>>
{
    public int  CondominiumId { get; set; }
    public Guid TenantId      { get; set; }

    public GetInstallmentsByCondominiumCommand() { }

    public GetInstallmentsByCondominiumCommand(int currentUserId, int condominiumId, Guid tenantId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        TenantId      = tenantId;
    }
}
