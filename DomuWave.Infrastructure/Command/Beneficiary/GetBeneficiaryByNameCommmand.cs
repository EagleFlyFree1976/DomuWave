using DomuWave.Services.Models.Dto.Beneficiary;
using DomuWave.Services.Models.Dto.Beneficiary;
using DomuWave.Services.Models.Dto.Beneficiary;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

public class GetBeneficiaryByNameCommmand : BaseBookRelatedCommand, IQuery<BeneficiaryReadDto>
{
    public GetBeneficiaryByNameCommmand()
    {
    }

    public GetBeneficiaryByNameCommmand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {

    }

    public string Name { get; set; }

}


