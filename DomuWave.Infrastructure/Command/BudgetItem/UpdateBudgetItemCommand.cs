using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BudgetItem;

public class UpdateBudgetItemCommand : BaseCommand, IQuery<BudgetItemReadDto>
{
    public int Id { get; set; }
    public UpdateBudgetItemDto Dto { get; set; }

    public UpdateBudgetItemCommand() { }
    public UpdateBudgetItemCommand(int currentUserId, int id, UpdateBudgetItemDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
