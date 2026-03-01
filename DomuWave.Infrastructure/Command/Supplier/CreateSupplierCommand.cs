using DomuWave.Services.Dto.Supplier;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class CreateSupplierCommand : BaseCommand, IQuery<SupplierReadDto>
{
    public Guid TenantId { get; set; }
    public CreateSupplierDto Dto { get; set; }

    public CreateSupplierCommand() { }

    public CreateSupplierCommand(int currentUserId) : base(currentUserId) { }
    public CreateSupplierCommand(int currentUserId, Guid tenantId, CreateSupplierDto dto) : base(currentUserId)
    {
        TenantId = tenantId;
        Dto      = dto;
    }
}
