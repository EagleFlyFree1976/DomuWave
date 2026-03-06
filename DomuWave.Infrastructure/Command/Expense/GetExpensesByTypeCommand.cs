using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpensesByTypeCommand : BaseCommand, IQuery<IList<ExpenseReadDto>>
{
    public int CondominiumId  { get; set; }
    public int ExpenseTypeId  { get; set; }

    public GetExpensesByTypeCommand() { }

    public GetExpensesByTypeCommand(int currentUserId) : base(currentUserId) { }
    public GetExpensesByTypeCommand(int currentUserId, int condominiumId, int expenseTypeId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        ExpenseTypeId = expenseTypeId;
    }
}
