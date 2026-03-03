using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class CloseBudgetCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public CloseBudgetCommand() { }
    public CloseBudgetCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
