using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class GetChartOfAccountsByIdCommand : BaseCommand, IQuery<ChartOfAccountsReadDto>
{
    public int Id { get; set; }

    public GetChartOfAccountsByIdCommand() { }
    public GetChartOfAccountsByIdCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
