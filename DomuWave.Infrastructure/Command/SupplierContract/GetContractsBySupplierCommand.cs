using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class GetContractsBySupplierCommand : BaseCommand, IQuery<IList<SupplierContractReadDto>>
{
    public int SupplierId { get; set; }

    public GetContractsBySupplierCommand() { }

    public GetContractsBySupplierCommand(int currentUserId, int supplierId) : base(currentUserId)
    {
        SupplierId = supplierId;
    }
}
