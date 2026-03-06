using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpensesByTypeCommand : BaseCommand, IQuery<IList<ExpenseReadDto>>
{
    public int CondominiumId { get; set; }
    public string ExpenseType { get; set; }

    public GetExpensesByTypeCommand() { }

    public GetExpensesByTypeCommand(int currentUserId) : base(currentUserId) { }
    public GetExpensesByTypeCommand(int currentUserId, int condominiumId, string expenseType) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        ExpenseType = expenseType;
    }
}
