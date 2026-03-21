using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class RecalculateBudgetItemsCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public RecalculateBudgetItemsCommand() { }
    public RecalculateBudgetItemsCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
