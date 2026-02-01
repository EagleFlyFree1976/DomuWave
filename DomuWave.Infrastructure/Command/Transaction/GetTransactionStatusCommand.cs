using DomuWave.Services.Models;
using CPQ.Core.DTO;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

public class GetTransactionStatusCommand : BaseCommand, IQuery<IList<LookupEntityDtoExtended<int>>>
{
    public GetTransactionStatusCommand()
    {
    }
    public GetTransactionStatusCommand(int currentUserId) : base(currentUserId)
    {
    }
}