using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

public class DisablePaymentMethodByIdCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public DisablePaymentMethodByIdCommand()
    {
    }

    public DisablePaymentMethodByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    
    public int PaymentMethodId { get; set; }
}