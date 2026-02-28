using DomuWave.Services.Dto.Condominium;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class CreateCondominiumCommand : BaseCommand, IQuery<CondominiumReadDto>
{
    public Guid TenantId { get; set; }
    public CreateCondominiumDto Dto { get; set; }

    public CreateCondominiumCommand() { }

    public CreateCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public CreateCondominiumCommand(int currentUserId, Guid tenantId, CreateCondominiumDto dto) : base(currentUserId)
    {
        TenantId = tenantId;
        Dto = dto;
    }
}
