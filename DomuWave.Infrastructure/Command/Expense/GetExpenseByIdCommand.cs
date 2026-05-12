using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpenseByIdCommand : BaseTenantRelatedCommand, IQuery<ExpenseReadDto>
{
    public long ExpenseId { get; set; }

    public GetExpenseByIdCommand() { }

    public GetExpenseByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetExpenseByIdCommand(int currentUserId, Guid tenantId, long expenseId) : base(currentUserId, tenantId)
    {
        ExpenseId = expenseId;
    }
}
