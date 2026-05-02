using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class PreApproveBudgetCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public PreApproveBudgetCommand() { }
    public PreApproveBudgetCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
