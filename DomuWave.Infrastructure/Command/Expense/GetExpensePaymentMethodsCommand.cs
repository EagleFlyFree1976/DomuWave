using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpensePaymentMethodsCommand : BaseCommand, IQuery<IList<ExpensePaymentMethodDto>>
{
    public GetExpensePaymentMethodsCommand() { }
    public GetExpensePaymentMethodsCommand(int currentUserId) : base(currentUserId) { }
}
