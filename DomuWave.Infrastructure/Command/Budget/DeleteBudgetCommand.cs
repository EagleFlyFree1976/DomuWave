using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class DeleteBudgetCommand : BaseTenantRelatedCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteBudgetCommand() { }
    public DeleteBudgetCommand(int currentUserId, Guid tenantId, int id) : base(currentUserId, tenantId)
        => Id = id;
}
