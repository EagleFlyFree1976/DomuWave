using DomuWave.Services.Models.Dto.Beneficiary;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

public class UpdateBeneficiaryCommand : BaseBookRelatedCommand, IQuery<BeneficiaryReadDto>
{
    public long BeneficiaryId { get; set; }
    public BeneficiaryCreateUpdateDto UpdateDto { get; set; }

    public UpdateBeneficiaryCommand()
    {
    }

    public UpdateBeneficiaryCommand(long beneficiaryId,int currentUserId, long bookId) : base(currentUserId, bookId)
    {
        BeneficiaryId = beneficiaryId;
    }


}