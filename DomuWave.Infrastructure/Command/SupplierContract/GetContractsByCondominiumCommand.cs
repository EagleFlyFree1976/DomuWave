using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class GetContractsByCondominiumCommand : BaseCommand, IQuery<IList<SupplierContractReadDto>>
{
    public int  CondominiumId { get; set; }
    public Guid TenantId      { get; set; }

    public GetContractsByCondominiumCommand() { }

    public GetContractsByCondominiumCommand(int currentUserId, int condominiumId, Guid tenantId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        TenantId      = tenantId;
    }
}
