using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BudgetItem;

public class CreateBudgetItemCommand : BaseCommand, IQuery<BudgetItemReadDto>
{
    public CreateBudgetItemDto Dto { get; set; }

    public CreateBudgetItemCommand() { }
    public CreateBudgetItemCommand(int currentUserId, CreateBudgetItemDto dto) : base(currentUserId)
        => Dto = dto;
}
