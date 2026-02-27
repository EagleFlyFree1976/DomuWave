using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetExpensesByDateRangeCommand : BaseCommand, IQuery<IList<Models.Expense>>
{
    public int CondominiumId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public GetExpensesByDateRangeCommand() { }

    public GetExpensesByDateRangeCommand(int currentUserId) : base(currentUserId) { }
    public GetExpensesByDateRangeCommand(int currentUserId, int condominiumId, DateTime from, DateTime to) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        From = from;
        To = to;
    }
}
