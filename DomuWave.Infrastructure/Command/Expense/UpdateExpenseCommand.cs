using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class UpdateExpenseCommand : BaseTenantRelatedCommand, IQuery<ExpenseReadDto>
{
    public long           ExpenseId { get; set; }
    public UpdateExpenseDto Dto     { get; set; }

    public UpdateExpenseCommand() { }
    public UpdateExpenseCommand(int currentUserId, Guid tenantId, long expenseId, UpdateExpenseDto dto) : base(currentUserId, tenantId)
    {
        ExpenseId = expenseId;
        Dto       = dto;
    }
}
