using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GetConsuntivoDetailCommand : BaseCommand, IQuery<ConsuntivoDetailDto>
{
    public int BudgetId { get; set; }

    public GetConsuntivoDetailCommand() { }
    public GetConsuntivoDetailCommand(int currentUserId, int budgetId) : base(currentUserId)
        => BudgetId = budgetId;
}
