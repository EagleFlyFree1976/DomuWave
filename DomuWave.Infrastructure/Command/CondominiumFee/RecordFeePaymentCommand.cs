using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class RecordFeePaymentCommand : BaseCommand, IQuery<bool>
{
    public long FeeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; }

    public RecordFeePaymentCommand() { }

    public RecordFeePaymentCommand(int currentUserId) : base(currentUserId) { }
    public RecordFeePaymentCommand(int currentUserId, long feeId, decimal amount, DateTime paymentDate, string paymentMethod) : base(currentUserId)
    {
        FeeId = feeId;
        Amount = amount;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;
    }
}
