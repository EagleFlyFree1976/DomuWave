using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class CreateExpenseCommand : BaseCommand, IQuery<Models.Expense>
{
    public Models.Expense Entity { get; set; }

    public CreateExpenseCommand() { }

    public CreateExpenseCommand(int currentUserId) : base(currentUserId) { }
    public CreateExpenseCommand(int currentUserId, Models.Expense entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
