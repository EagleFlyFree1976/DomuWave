using DomuWave.Services.Models.Dto.Beneficiary;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Beneficiary;

public class FindBeneficiaryCommand : BaseBookRelatedCommand, IQuery<IList<BeneficiaryReadDto>>
{
    public FindBeneficiaryCommand()
    {
    }

    public FindBeneficiaryCommand(int currentUserId, long bookId, string q) : base(currentUserId, bookId)
    {
        BookId = bookId;
        Q = q;
    }

    public long BookId { get; set; }


    public string Q { get; set; }
    public bool AddIfNotExists { get; set; } = false;
}