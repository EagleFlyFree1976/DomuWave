using DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;

public class UpdateChartOfAccountsCategoryTemplateCommand : BaseCommand, IQuery<ChartOfAccountsCategoryTemplateReadDto>
{
    public int                                      Id  { get; set; }
    public UpdateChartOfAccountsCategoryTemplateDto Dto { get; set; }

    public UpdateChartOfAccountsCategoryTemplateCommand() { }
    public UpdateChartOfAccountsCategoryTemplateCommand(int currentUserId, int id, UpdateChartOfAccountsCategoryTemplateDto dto)
        : base(currentUserId) { Id = id; Dto = dto; }
}
