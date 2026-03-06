using DomuWave.Services.Dto.Expense;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class CreateExpenseCommand : BaseCommand, IQuery<ExpenseReadDto>
{
    public CreateExpenseDto Dto { get; set; }

    public CreateExpenseCommand() { }
    public CreateExpenseCommand(int currentUserId, CreateExpenseDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
