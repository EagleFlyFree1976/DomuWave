using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BudgetItem;

public class DeleteBudgetItemCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteBudgetItemCommand() { }
    public DeleteBudgetItemCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
