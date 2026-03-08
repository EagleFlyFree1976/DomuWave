using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class SearchUnitOwnersCommand : BaseCommand, IQuery<IList<UnitOwnerReadDto>>
{
    public string Query { get; set; }
    public Guid TenantId { get; set; }
    public SearchUnitOwnersCommand()
    {
        
    }

    public SearchUnitOwnersCommand(int currentUserId, string query, Guid tenantId) : base(currentUserId)
    {
        Query = query;
        TenantId = tenantId;
    }
}
