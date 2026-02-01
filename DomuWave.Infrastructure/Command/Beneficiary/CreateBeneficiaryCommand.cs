using DomuWave.Services.Models.Dto.Beneficiary;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

/// <summary>
/// Crea un nuovo beneficiario con i parametri impostati
/// </summary>
public class CreateBeneficiaryCommand : BaseBookRelatedCommand, IQuery<BeneficiaryReadDto>
{
    public CreateBeneficiaryCommand()
    {
    }

    public CreateBeneficiaryCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public BeneficiaryCreateUpdateDto CreateDto { get; set; }

}