using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class ApplyChartOfAccountsTemplateToCondominiumCommand : BaseCommand, IQuery<int>
{
    public int TemplateId    { get; set; }
    public int CondominiumId { get; set; }
    public ApplyChartOfAccountsTemplateToCondominiumCommand() { }
    public ApplyChartOfAccountsTemplateToCondominiumCommand(int currentUserId, int templateId, int condominiumId)
        : base(currentUserId) { TemplateId = templateId; CondominiumId = condominiumId; }
}
