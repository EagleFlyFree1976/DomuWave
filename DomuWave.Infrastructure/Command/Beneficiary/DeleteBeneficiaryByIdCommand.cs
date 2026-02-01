using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

public class DeleteBeneficiaryByIdCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public DeleteBeneficiaryByIdCommand()
    {
    }

    public DeleteBeneficiaryByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }


    public long BeneficiaryId { get; set; }
}