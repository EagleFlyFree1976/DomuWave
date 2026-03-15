using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class ReopenBudgetCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public ReopenBudgetCommand() { }
    public ReopenBudgetCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
