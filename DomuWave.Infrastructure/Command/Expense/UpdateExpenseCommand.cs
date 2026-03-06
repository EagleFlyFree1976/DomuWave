using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class UpdateExpenseCommand : BaseCommand, IQuery<ExpenseReadDto>
{
    public long           ExpenseId { get; set; }
    public UpdateExpenseDto Dto     { get; set; }

    public UpdateExpenseCommand() { }
    public UpdateExpenseCommand(int currentUserId, long expenseId, UpdateExpenseDto dto) : base(currentUserId)
    {
        ExpenseId = expenseId;
        Dto       = dto;
    }
}
