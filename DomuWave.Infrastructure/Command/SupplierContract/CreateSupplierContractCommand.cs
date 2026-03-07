using DomuWave.Services.Dto.SupplierContract;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.SupplierContract;

public class CreateSupplierContractCommand : BaseCommand, IQuery<SupplierContractReadDto>
{
    public CreateSupplierContractDto Dto { get; set; }

    public CreateSupplierContractCommand() { }

    public CreateSupplierContractCommand(int currentUserId, CreateSupplierContractDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
