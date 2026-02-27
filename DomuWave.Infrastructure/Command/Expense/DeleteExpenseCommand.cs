using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class DeleteExpenseCommand : BaseCommand, IQuery<bool>
{
    public long ExpenseId { get; set; }

    public DeleteExpenseCommand() { }

    public DeleteExpenseCommand(int currentUserId) : base(currentUserId) { }
    public DeleteExpenseCommand(int currentUserId, long expenseId) : base(currentUserId)
    {
        ExpenseId = expenseId;
    }
}
