using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class UpdateBudgetCommand : BaseCommand, IQuery<BudgetReadDto>
{
    public int Id { get; set; }
    public UpdateBudgetDto Dto { get; set; }

    public UpdateBudgetCommand() { }
    public UpdateBudgetCommand(int currentUserId, int id, UpdateBudgetDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
