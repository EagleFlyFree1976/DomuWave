using DomuWave.Services.Dto.UnitTenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitTenant;

public class GetUnitTenantByIdCommand : BaseCommand, IQuery<UnitTenantReadDto>
{
    public int Id { get; set; }

    public GetUnitTenantByIdCommand() { }

    public GetUnitTenantByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetUnitTenantByIdCommand(int currentUserId, int id) : base(currentUserId)
    {
        Id = id;
    }
}
