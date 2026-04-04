using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class FixOrphanBudgetItemsCommand : BaseCommand, IQuery<int>
{
    public int Id { get; set; }

    public FixOrphanBudgetItemsCommand() { }
    public FixOrphanBudgetItemsCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
