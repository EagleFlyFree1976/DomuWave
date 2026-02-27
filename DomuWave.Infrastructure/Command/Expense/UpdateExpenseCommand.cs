using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class UpdateExpenseCommand : BaseCommand, IQuery<Models.Expense>
{
    public long ExpenseId { get; set; }
    public Models.Expense Entity { get; set; }

    public UpdateExpenseCommand() { }

    public UpdateExpenseCommand(int currentUserId) : base(currentUserId) { }
    public UpdateExpenseCommand(int currentUserId, long expenseId, Models.Expense entity) : base(currentUserId)
    {
        ExpenseId = expenseId;
        Entity = entity;
    }
}
