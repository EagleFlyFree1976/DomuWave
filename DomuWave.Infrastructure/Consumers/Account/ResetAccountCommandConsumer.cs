using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using Bogus;
using Bogus.DataSets;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using MassTransit.Mediator;
using SimpleMediator.Core;
using IMediator = SimpleMediator.Core.IMediator;

namespace DomuWave.Services.Consumers.Book;

public   class ResetAccountCommandConsumer : InMemoryConsumerBase<ResetAccountCommand,bool>
{
    private IMediator _mediator;
    private IUserService _userService;
    public ResetAccountCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IMediator mediator, IUserService userService) : base(sessionFactoryProvider)
    {
        _mediator = mediator;
        _userService = userService;
    }

    protected override async Task<bool> Consume(ResetAccountCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        CanAccessToBookCommand canAccessToBookCommand = new CanAccessToBookCommand(currentUser.Id, evt.BookId);
        if (!await _mediator.GetResponse(canAccessToBookCommand, cancellationToken).ConfigureAwait(false))
        {
            throw new NotAllowedOperationException("Non hai accesso alla risorsa richiesta");
        }

        var account = await session.GetAsync<Models.Account>(evt.AccountId, cancellationToken).ConfigureAwait(false);

        if (account == null || account.IsDeleted || account.Book.Id != evt.BookId)
        {
            throw new NotFoundException("Account non trovato");
        }

        await session.CreateQuery("delete from DomuWave.Services.Models.AccountReport a where a.Account.Id=:accountId").SetParameter("accountId", evt.AccountId).ExecuteUpdateAsync(cancellationToken);
        await session.CreateQuery("delete from DomuWave.Services.Models.Transaction a where a.DestinationAccount.Id=:accountId").SetParameter("accountId", evt.AccountId).ExecuteUpdateAsync(cancellationToken);
        await session.CreateQuery("delete from DomuWave.Services.Models.Transaction a where a.Account.Id=:accountId").SetParameter("accountId", evt.AccountId).ExecuteUpdateAsync(cancellationToken);

        RecalculateAccountBalanceCommand recalculateAccountBalanceCommand = new RecalculateAccountBalanceCommand();
        recalculateAccountBalanceCommand.AccountId = evt.AccountId;
        recalculateAccountBalanceCommand.BookId = evt.BookId;
        recalculateAccountBalanceCommand.CurrentUserId = evt.CurrentUserId;
        await _mediator.PublicAsync(recalculateAccountBalanceCommand, cancellationToken).ConfigureAwait(false);

        return true;

    }
}