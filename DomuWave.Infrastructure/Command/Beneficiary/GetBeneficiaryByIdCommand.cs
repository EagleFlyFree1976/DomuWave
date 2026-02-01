using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Beneficiary;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

/// <summary>
///  Ritorna il dettaglio dell'account selezionato
/// </summary>
public class GetBeneficiaryByIdCommand : BaseBookRelatedCommand, IQuery<BeneficiaryReadDto>
{
    public GetBeneficiaryByIdCommand()
    {
    }

    public GetBeneficiaryByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
        
    }
   
    public long BeneficiaryId { get; set; }
    
}