using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GenerateInstallmentsFromBudgetCommand : BaseCommand, IQuery<bool>
{
    public int      BudgetId             { get; set; }
    public int      NumberOfInstallments { get; set; } = 4;
    public DateTime FirstDueDate         { get; set; } = DateTime.Today;
    /// <summary>Se true, elimina le rate esistenti prima di rigenerare.</summary>
    public bool     Force                { get; set; } = false;

    public GenerateInstallmentsFromBudgetCommand() { }
    public GenerateInstallmentsFromBudgetCommand(int currentUserId, int budgetId,
        int numberOfInstallments, DateTime firstDueDate, bool force = false) : base(currentUserId)
    {
        BudgetId             = budgetId;
        NumberOfInstallments = numberOfInstallments;
        FirstDueDate         = firstDueDate;
        Force                = force;
    }
}
