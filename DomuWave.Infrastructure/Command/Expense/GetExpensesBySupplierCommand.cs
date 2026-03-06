using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpensesBySupplierCommand : BaseCommand, IQuery<IList<ExpenseReadDto>>
{
    public int SupplierId { get; set; }

    public GetExpensesBySupplierCommand() { }

    public GetExpensesBySupplierCommand(int currentUserId) : base(currentUserId) { }
    public GetExpensesBySupplierCommand(int currentUserId, int supplierId) : base(currentUserId)
    {
        SupplierId = supplierId;
    }
}
