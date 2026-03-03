using DomuWave.Services.Dto.Budget;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Budget;

public class GetBudgetByIdCommand : BaseCommand, IQuery<BudgetReadDto>
{
    public int Id { get; set; }

    public GetBudgetByIdCommand() { }
    public GetBudgetByIdCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
