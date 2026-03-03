using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GetBudgetsByCondominiumCommand : BaseCommand, IQuery<IList<BudgetReadDto>>
{
    public int CondominiumId { get; set; }

    public GetBudgetsByCondominiumCommand() { }
    public GetBudgetsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
