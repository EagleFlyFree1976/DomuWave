using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class GetChartOfAccountsByCondominiumCommand : BaseCommand, IQuery<IList<ChartOfAccountsReadDto>>
{
    public int  CondominiumId { get; set; }
    public Guid TenantId      { get; set; }

    public GetChartOfAccountsByCondominiumCommand() { }
    public GetChartOfAccountsByCondominiumCommand(int currentUserId, int condominiumId, Guid tenantId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        TenantId      = tenantId;
    }
}
