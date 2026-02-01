using DomuWave.Services.Models.Dto.Beneficiary;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

public class CreateBeneficiaryByNameCommand : BaseBookRelatedCommand, IQuery<BeneficiaryReadDto>
{
    public CreateBeneficiaryByNameCommand()
    {
    }

    public CreateBeneficiaryByNameCommand(int currentUserId, long bookId) : base(currentUserId, bookId)
    {
    }

    public BeneficiaryCreateByNameDto CreateDto { get; set; }
}