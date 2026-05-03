using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BudgetItem;

public class GetBudgetItemsByBudgetCommand : BaseCommand, IQuery<IList<BudgetItemReadDto>>
{
    public int  BudgetId { get; set; }
    public Guid TenantId { get; set; }

    public GetBudgetItemsByBudgetCommand() { }
    public GetBudgetItemsByBudgetCommand(int currentUserId, int budgetId, Guid tenantId) : base(currentUserId)
    {
        BudgetId = budgetId;
        TenantId = tenantId;
    }
}
