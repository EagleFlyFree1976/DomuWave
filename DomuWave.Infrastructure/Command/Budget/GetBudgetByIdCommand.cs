using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GetBudgetByIdCommand : BaseTenantRelatedCommand, IQuery<BudgetReadDto>
{
    public int Id { get; set; }

    public GetBudgetByIdCommand() { }
    public GetBudgetByIdCommand(int currentUserId, Guid tenantId, int id) : base(currentUserId, tenantId)
        => Id = id;
}
