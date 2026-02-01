using DomuWave.Services.Helper;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Transaction;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

public class FindTransactionsCommand : BasePagedCommand, IQuery<PagedResult<TransactionReadDto>>
{
    public FindTransaction Filters { get; set; }
}



