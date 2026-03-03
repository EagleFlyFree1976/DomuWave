using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class GetChartOfAccountsByCondominiumCommand : BaseCommand, IQuery<IList<ChartOfAccountsReadDto>>
{
    public int CondominiumId { get; set; }

    public GetChartOfAccountsByCondominiumCommand() { }
    public GetChartOfAccountsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
