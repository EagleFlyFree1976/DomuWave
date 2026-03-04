using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class UpdateChartOfAccountsCommand : BaseCommand, IQuery<ChartOfAccountsReadDto>
{
    public int                      Id  { get; set; }
    public UpdateChartOfAccountsDto Dto { get; set; }

    public UpdateChartOfAccountsCommand() { }
    public UpdateChartOfAccountsCommand(int currentUserId, int id, UpdateChartOfAccountsDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
