using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpensesByCondominiumCommand : BaseCommand, IQuery<IList<Models.Expense>>
{
    public int CondominiumId { get; set; }

    public GetExpensesByCondominiumCommand() { }

    public GetExpensesByCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public GetExpensesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
