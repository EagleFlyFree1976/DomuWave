using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.PaymentMethod;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

/// <summary>
/// ritorna l'elenco di tutti i metodi di pagamento fanno riferimento ad un dato book oppure tutti quelli di default
/// </summary>
public class FindPaymentMethodCommand : BaseBookRelatedCommand, IQuery<IList<PaymentMethodReadDto>>
{
    public FindPaymentMethodCommand()
    {
    }

    public FindPaymentMethodCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public long BookId { get; set; }
    
}