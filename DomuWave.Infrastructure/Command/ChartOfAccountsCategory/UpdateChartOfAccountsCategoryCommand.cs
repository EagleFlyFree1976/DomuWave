using DomuWave.Services.Dto.ChartOfAccountsCategory;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategory;

public class UpdateChartOfAccountsCategoryCommand : BaseCommand, IQuery<ChartOfAccountsCategoryReadDto>
{
    public int                             Id  { get; set; }
    public UpdateChartOfAccountsCategoryDto Dto { get; set; }

    public UpdateChartOfAccountsCategoryCommand() { }
    public UpdateChartOfAccountsCategoryCommand(int currentUserId, int id, UpdateChartOfAccountsCategoryDto dto)
        : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
