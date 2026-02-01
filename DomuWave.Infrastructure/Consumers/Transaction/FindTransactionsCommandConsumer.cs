using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Extensions;
using DomuWave.Services.Helper;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Transaction;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class FindTransactionsCommandConsumer : InMemoryConsumerBase<FindTransactionsCommand, PagedResult<TransactionReadDto>>
{
    private ITransactionService _transactionService;
    private IUserService _userService;
    public FindTransactionsCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ITransactionService transactionService, IUserService userService) : base(sessionFactoryProvider)
    {
        _transactionService = transactionService;
        _userService = userService;
    }
    protected override async Task<PagedResult<TransactionReadDto>> Consume(FindTransactionsCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {

     



        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var result = await _transactionService.Find(evt.Filters, evt.PageNumber, evt.PageSize, evt.SortField, evt.Asc, currentUser, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return null;
        return new PagedResult<TransactionReadDto>
        {
            PageNumber = evt.PageNumber,
            PageSize = evt.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => x.ToDto(30)).ToList()
        };
        
        
    }
}