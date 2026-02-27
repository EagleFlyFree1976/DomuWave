using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Expense;

public class MarkExpenseAsPaidCommand : BaseCommand, IQuery<bool>
{
    public long ExpenseId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; }

    public MarkExpenseAsPaidCommand() { }

    public MarkExpenseAsPaidCommand(int currentUserId) : base(currentUserId) { }
    public MarkExpenseAsPaidCommand(int currentUserId, long expenseId, DateTime paymentDate, string paymentMethod) : base(currentUserId)
    {
        ExpenseId = expenseId;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;
    }
}
