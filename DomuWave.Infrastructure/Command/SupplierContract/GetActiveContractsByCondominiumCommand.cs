using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class GetActiveContractsByCondominiumCommand : BaseCommand, IQuery<IList<SupplierContractReadDto>>
{
    public int CondominiumId { get; set; }

    public GetActiveContractsByCondominiumCommand() { }

    public GetActiveContractsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
