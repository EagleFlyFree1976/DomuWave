using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

/// <summary>
/// Rigenera tutte le ExpenseAllocation per le spese dell'esercizio associato al budget consuntivo.
/// </summary>
public class RegenerateExpenseAllocationsCommand : BaseCommand, IQuery<int>
{
    /// <summary>Id del budget consuntivo.</summary>
    public int BudgetId { get; set; }

    public RegenerateExpenseAllocationsCommand() { }
    public RegenerateExpenseAllocationsCommand(int currentUserId, int budgetId) : base(currentUserId)
        => BudgetId = budgetId;
}
