using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetUnpaidExpensesCommand : BaseCommand, IQuery<IList<Models.Expense>>
{
    public int CondominiumId { get; set; }

    public GetUnpaidExpensesCommand() { }

    public GetUnpaidExpensesCommand(int currentUserId) : base(currentUserId) { }
    public GetUnpaidExpensesCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
