using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpenseByIdCommand : BaseCommand, IQuery<Models.Expense>
{
    public long ExpenseId { get; set; }

    public GetExpenseByIdCommand() { }

    public GetExpenseByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetExpenseByIdCommand(int currentUserId, long expenseId) : base(currentUserId)
    {
        ExpenseId = expenseId;
    }
}
