using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class DeleteExpenseCommand : BaseTenantRelatedCommand, IQuery<bool>
{
    public long ExpenseId { get; set; }

    public DeleteExpenseCommand() { }

    public DeleteExpenseCommand(int currentUserId) : base(currentUserId) { }
    public DeleteExpenseCommand(int currentUserId, Guid tenantId, long expenseId) : base(currentUserId, tenantId)
    {
        ExpenseId = expenseId;
    }
}
