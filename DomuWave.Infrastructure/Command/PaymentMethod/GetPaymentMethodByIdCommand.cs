using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.PaymentMethod;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

/// <summary>
///  Ritorna il dettaglio dell'account selezionato
/// </summary>
public class GetPaymentMethodByIdCommand : BaseBookRelatedCommand, IQuery<PaymentMethodReadDto>
{
    public GetPaymentMethodByIdCommand()
    {
    }

    public GetPaymentMethodByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
        
    }
   
    public int PaymentMethodId { get; set; }
    
}