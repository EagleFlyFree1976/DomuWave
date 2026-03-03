using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class DeleteBudgetCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteBudgetCommand() { }
    public DeleteBudgetCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
