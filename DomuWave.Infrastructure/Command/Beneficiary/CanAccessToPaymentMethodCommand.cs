using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

public class CanAccessToBeneficiaryCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public long? BeneficiaryId { get; set; }
    public CanAccessToBeneficiaryCommand()
    {
    }

    public CanAccessToBeneficiaryCommand(long? beneficiaryId, int currentUserId, long bookId) : base(currentUserId, bookId)
    {
        this.BeneficiaryId = beneficiaryId;
    }
}