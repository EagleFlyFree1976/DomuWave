using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class RegenerateExpenseAllocationsCommand : BaseCommand, IQuery<RegenerateExpenseAllocationsResult>
{
    public int BudgetId { get; set; }

    public RegenerateExpenseAllocationsCommand() { }
    public RegenerateExpenseAllocationsCommand(int currentUserId, int budgetId) : base(currentUserId)
        => BudgetId = budgetId;
}

public class RegenerateExpenseAllocationsResult
{
    public int    BudgetId                  { get; set; }
    public int    CondominiumId             { get; set; }
    public int    FiscalYearId              { get; set; }
    public int    ChargesWithoutExpenseCount { get; set; }
    public int    CreatedExpensesCount      { get; set; }
    public int    ExpensesCount             { get; set; }
    public int    ProcessedCount            { get; set; }
    public List<string> DiagCharges         { get; set; } = [];
    public List<string> DiagMessages        { get; set; } = [];
}
