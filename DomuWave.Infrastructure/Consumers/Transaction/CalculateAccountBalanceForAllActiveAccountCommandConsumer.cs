using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class CalculateAccountBalanceForAllActiveAccountCommandConsumer : InMemoryConsumerBase<CalculateAccountBalanceForAllActiveAccountCommand, bool>
{
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;

    public CalculateAccountBalanceForAllActiveAccountCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IMediator mediator, IAccountService accountService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _mediator = mediator;
        _accountService = accountService;
    }

    protected override async Task<bool> Consume(CalculateAccountBalanceForAllActiveAccountCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var allAccounts = await _accountService.GetAll(currentUser, cancellationToken).ConfigureAwait(false);
        foreach (Models.Account account in allAccounts.Where(k=>k.IsActive && !k.IsDeleted))
        {
            RecalculateAccountBalanceCommand accountBalanceCommand = new RecalculateAccountBalanceCommand()
            {
                BookId = account.Book.Id,
                AccountId = account.Id,
                CurrentUserId = evt.CurrentUserId
            };
            await _mediator.PublicAsync(accountBalanceCommand, cancellationToken).ConfigureAwait(false);

        }

        return true;
    }
}