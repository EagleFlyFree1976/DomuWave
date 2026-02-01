using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

public class EnablePaymentMethodByIdCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public EnablePaymentMethodByIdCommand()
    {
    }

    public EnablePaymentMethodByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    
    public int PaymentMethodId { get; set; }
}