using DomuWave.Services.Dto.UnitTenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitTenant;

public class SearchUnitTenantsCommand : BaseCommand, IQuery<IList<UnitTenantReadDto>>
{
    public string Query { get; set; }
    public Guid TenantId { get; set; }
    public SearchUnitTenantsCommand()
    {
        
    }

    public SearchUnitTenantsCommand(int currentUserId, string query, Guid tenantId) : base(currentUserId)
    {
        Query = query;
        TenantId = tenantId;
    }
}
