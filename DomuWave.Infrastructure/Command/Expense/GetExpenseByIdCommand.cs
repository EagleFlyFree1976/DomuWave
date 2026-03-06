using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpenseByIdCommand : BaseCommand, IQuery<ExpenseReadDto>
{
    public long ExpenseId { get; set; }

    public GetExpenseByIdCommand() { }

    public GetExpenseByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetExpenseByIdCommand(int currentUserId, long expenseId) : base(currentUserId)
    {
        ExpenseId = expenseId;
    }
}
