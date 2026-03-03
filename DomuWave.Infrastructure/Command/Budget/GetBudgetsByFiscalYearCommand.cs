using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GetBudgetsByFiscalYearCommand : BaseCommand, IQuery<IList<BudgetReadDto>>
{
    public int FiscalYearId { get; set; }

    public GetBudgetsByFiscalYearCommand() { }
    public GetBudgetsByFiscalYearCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
        => FiscalYearId = fiscalYearId;
}
