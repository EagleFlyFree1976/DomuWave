using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

public class DeletePaymentMethodByIdCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public DeletePaymentMethodByIdCommand()
    {
    }

    public DeletePaymentMethodByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    
    public int PaymentMethodId { get; set; }
}