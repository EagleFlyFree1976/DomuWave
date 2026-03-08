using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GenerateInstallmentsFromBudgetCommand : BaseCommand, IQuery<bool>
{
    public int      BudgetId             { get; set; }
    public int      NumberOfInstallments { get; set; } = 4;
    public DateTime FirstDueDate         { get; set; } = DateTime.Today;

    public GenerateInstallmentsFromBudgetCommand() { }
    public GenerateInstallmentsFromBudgetCommand(int currentUserId, int budgetId,
        int numberOfInstallments, DateTime firstDueDate) : base(currentUserId)
    {
        BudgetId             = budgetId;
        NumberOfInstallments = numberOfInstallments;
        FirstDueDate         = firstDueDate;
    }
}
