using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class CreateBudgetCommand : BaseCommand, IQuery<BudgetReadDto>
{
    public CreateBudgetDto Dto { get; set; }

    public CreateBudgetCommand() { }
    public CreateBudgetCommand(int currentUserId, CreateBudgetDto dto) : base(currentUserId)
        => Dto = dto;
}
