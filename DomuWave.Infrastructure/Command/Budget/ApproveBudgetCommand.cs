using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class ApproveBudgetCommand : BaseCommand, IQuery<bool>
{
    public int      Id                   { get; set; }
    public int      NumberOfInstallments { get; set; } = 4;
    public DateTime FirstDueDate         { get; set; } = DateTime.Today;

    public ApproveBudgetCommand() { }
    public ApproveBudgetCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
