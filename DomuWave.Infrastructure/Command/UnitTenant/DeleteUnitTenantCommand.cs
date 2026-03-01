using DomuWave.Services.Dto.UnitTenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitTenant;

public class DeleteUnitTenantCommand : BaseCommand, IQuery<UnitTenantReadDto>
{
    public int Id { get; set; }

    public DeleteUnitTenantCommand() { }

    public DeleteUnitTenantCommand(int currentUserId) : base(currentUserId) { }
    public DeleteUnitTenantCommand(int currentUserId, int id) : base(currentUserId)
    {
        Id = id;
    }
}
