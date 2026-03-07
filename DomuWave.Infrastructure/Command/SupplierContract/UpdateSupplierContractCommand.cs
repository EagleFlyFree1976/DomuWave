using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class UpdateSupplierContractCommand : BaseCommand, IQuery<SupplierContractReadDto>
{
    public int                      ContractId { get; set; }
    public UpdateSupplierContractDto Dto       { get; set; }

    public UpdateSupplierContractCommand() { }

    public UpdateSupplierContractCommand(int currentUserId, int contractId, UpdateSupplierContractDto dto) : base(currentUserId)
    {
        ContractId = contractId;
        Dto        = dto;
    }
}
