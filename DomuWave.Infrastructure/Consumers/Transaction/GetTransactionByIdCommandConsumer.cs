using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Transaction;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class GetTransactionByIdCommandConsumer : InMemoryConsumerBase<GetTransactionByIdCommand, TransactionReadDto>
{
    private ITransactionService _transactionService;
    private IUserService _userService;

    public GetTransactionByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ITransactionService transactionService, IUserService userService) : base(sessionFactoryProvider)
    {
        _transactionService = transactionService;
        _userService = userService;
    }

    protected override async Task<TransactionReadDto> Consume(GetTransactionByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);
        var transactionCreated = await _transactionService.GetById(evt.Id, evt.BookId, currentUser, cancellationToken).ConfigureAwait(false);
        return transactionCreated.ToDto();
    }
}


public class DeleteTransactionByIdCommandConsumer : InMemoryConsumerBase<DeleteTransactionByIdCommand, bool>
{
    private ITransactionService _transactionService;
    private IUserService _userService;

    public DeleteTransactionByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ITransactionService transactionService, IUserService userService) : base(sessionFactoryProvider)
    {
        _transactionService = transactionService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(DeleteTransactionByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);
        await _transactionService.Delete(evt.Id, evt.BookId, currentUser, cancellationToken).ConfigureAwait(false);
        return true;
    }
}