using DomuWave.Services.Dto.Supplier;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class UpdateSupplierCommand : BaseCommand, IQuery<SupplierReadDto>
{
    public int SupplierId { get; set; }
    public UpdateSupplierDto Dto { get; set; }

    public UpdateSupplierCommand() { }

    public UpdateSupplierCommand(int currentUserId) : base(currentUserId) { }
    public UpdateSupplierCommand(int currentUserId, int supplierId, UpdateSupplierDto dto) : base(currentUserId)
    {
        SupplierId = supplierId;
        Dto        = dto;
    }
}
