using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategory;

public class ImportCategoriesFromTemplateCommand : BaseCommand, IQuery<int>
{
    public Guid        TenantId    { get; set; }
    public IList<int>  TemplateIds { get; set; } = [];

    public ImportCategoriesFromTemplateCommand() { }
    public ImportCategoriesFromTemplateCommand(int currentUserId, Guid tenantId, IList<int> templateIds)
        : base(currentUserId)
    {
        TenantId    = tenantId;
        TemplateIds = templateIds;
    }
}
