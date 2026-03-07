using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class GetContractByIdCommand : BaseCommand, IQuery<SupplierContractReadDto>
{
    public int ContractId { get; set; }

    public GetContractByIdCommand() { }

    public GetContractByIdCommand(int currentUserId, int contractId) : base(currentUserId)
    {
        ContractId = contractId;
    }
}
