using DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;

public class CreateChartOfAccountsCategoryTemplateCommand : BaseCommand, IQuery<ChartOfAccountsCategoryTemplateReadDto>
{
    public CreateChartOfAccountsCategoryTemplateDto Dto { get; set; }

    public CreateChartOfAccountsCategoryTemplateCommand() { }
    public CreateChartOfAccountsCategoryTemplateCommand(int currentUserId, CreateChartOfAccountsCategoryTemplateDto dto)
        : base(currentUserId) => Dto = dto;
}
