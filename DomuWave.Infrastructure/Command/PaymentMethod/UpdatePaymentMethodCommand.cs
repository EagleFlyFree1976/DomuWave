using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.PaymentMethod;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

public class UpdatePaymentMethodCommand : BaseBookRelatedCommand,IQuery<PaymentMethodReadDto>
{
    public UpdatePaymentMethodCommand()
    {
    }
    public int PaymentMethodId { get; set; }

    public UpdatePaymentMethodCommand(int paymentMethodId,int currentUserId, long bookId ) : base(currentUserId, bookId)
    {
        PaymentMethodId = paymentMethodId;
    }

    public PaymentMethodCreateUpdateDto UpdateDto { get; set; }
        
}