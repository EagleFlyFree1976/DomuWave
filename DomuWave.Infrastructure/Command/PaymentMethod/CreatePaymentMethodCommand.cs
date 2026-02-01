using DomuWave.Services.Models.Dto.PaymentMethod;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod
{
    /// <summary>
    /// Crea un nuovo metodo di pagamento con im parametri impostati
    /// </summary>
    public class CreatePaymentMethodCommand : BaseBookRelatedCommand,IQuery<PaymentMethodReadDto>
    {
        public CreatePaymentMethodCommand()
        {
        }

        public CreatePaymentMethodCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
        {
        }

        public PaymentMethodCreateUpdateDto CreateDto { get; set; }
        
    }
}
