using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class GetTotalExpensesCommand : BaseCommand, IQuery<decimal>
{
    public int CondominiumId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public GetTotalExpensesCommand() { }

    public GetTotalExpensesCommand(int currentUserId) : base(currentUserId) { }
    public GetTotalExpensesCommand(int currentUserId, int condominiumId, DateTime from, DateTime to) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        From = from;
        To = to;
    }
}
