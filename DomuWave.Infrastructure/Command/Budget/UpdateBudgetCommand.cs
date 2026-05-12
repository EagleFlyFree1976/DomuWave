using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class UpdateBudgetCommand : BaseTenantRelatedCommand, IQuery<BudgetReadDto>
{
    public int Id { get; set; }
    public UpdateBudgetDto Dto { get; set; }

    public UpdateBudgetCommand() { }
    public UpdateBudgetCommand(int currentUserId, Guid tenantId, int id, UpdateBudgetDto dto) : base(currentUserId, tenantId)
    {
        Id  = id;
        Dto = dto;
    }
}
