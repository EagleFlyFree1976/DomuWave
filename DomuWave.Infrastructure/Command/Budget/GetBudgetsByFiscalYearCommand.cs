using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GetBudgetsByFiscalYearCommand : BaseCommand, IQuery<IList<BudgetReadDto>>
{
    public int  FiscalYearId { get; set; }
    public Guid TenantId     { get; set; }

    public GetBudgetsByFiscalYearCommand() { }
    public GetBudgetsByFiscalYearCommand(int currentUserId, int fiscalYearId, Guid tenantId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
        TenantId     = tenantId;
    }
}
