using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class GetContractsByCondominiumCommand : BaseCommand, IQuery<IList<SupplierContractReadDto>>
{
    public int CondominiumId { get; set; }

    public GetContractsByCondominiumCommand() { }

    public GetContractsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
