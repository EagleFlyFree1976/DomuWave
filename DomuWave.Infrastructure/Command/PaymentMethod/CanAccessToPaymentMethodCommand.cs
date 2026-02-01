using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

public class CanAccessToPaymentMethodCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public int? PaymentMethodId { get; set; }
    public CanAccessToPaymentMethodCommand()
    {
    }

    public CanAccessToPaymentMethodCommand(int? paymentMethodId, int currentUserId, long bookId) : base(currentUserId, bookId)
    {
        this.PaymentMethodId = paymentMethodId;
    }
}