using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class CreateChartOfAccountsCommand : BaseCommand, IQuery<ChartOfAccountsReadDto>
{
    public CreateChartOfAccountsDto Dto { get; set; }

    public CreateChartOfAccountsCommand() { }
    public CreateChartOfAccountsCommand(int currentUserId, CreateChartOfAccountsDto dto) : base(currentUserId)
        => Dto = dto;
}
