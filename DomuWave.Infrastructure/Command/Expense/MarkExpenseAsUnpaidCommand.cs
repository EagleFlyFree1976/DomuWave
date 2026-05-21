using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class MarkExpenseAsUnpaidCommand : BaseCommand, IQuery<bool>
{
    public long ExpenseId { get; set; }

    public MarkExpenseAsUnpaidCommand() { }

    public MarkExpenseAsUnpaidCommand(int currentUserId) : base(currentUserId) { }

    public MarkExpenseAsUnpaidCommand(int currentUserId, long expenseId) : base(currentUserId)
    {
        ExpenseId = expenseId;
    }
}
