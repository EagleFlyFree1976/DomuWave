using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Supplier;

public class SearchSuppliersCommand : BaseCommand, IQuery<IList<Models.Supplier>>
{
    public Guid TenantId { get; set; }
    public string Query { get; set; }

    public SearchSuppliersCommand() { }

    public SearchSuppliersCommand(int currentUserId) : base(currentUserId) { }
    public SearchSuppliersCommand(int currentUserId, Guid tenantId, string query) : base(currentUserId)
    {
        TenantId = tenantId;
        Query = query;
    }
}
