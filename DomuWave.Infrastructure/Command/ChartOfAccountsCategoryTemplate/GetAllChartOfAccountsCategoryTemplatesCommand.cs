using DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;

public class GetAllChartOfAccountsCategoryTemplatesCommand : BaseCommand, IQuery<IList<ChartOfAccountsCategoryTemplateReadDto>>
{
    public GetAllChartOfAccountsCategoryTemplatesCommand() { }
    public GetAllChartOfAccountsCategoryTemplatesCommand(int currentUserId) : base(currentUserId) { }
}
