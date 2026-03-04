using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteChartOfAccountsCommandConsumer
    : InMemoryConsumerBase<DeleteChartOfAccountsCommand, bool>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public DeleteChartOfAccountsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IChartOfAccountsService accountService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService    = userService;
    }

    protected override async Task<bool> Consume(
        DeleteChartOfAccountsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _accountService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Conto non trovato.");

        var hasChildren = await session.Query<ChartOfAccounts>()
            .AnyAsync(x => x.ParentAccount.Id == command.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (hasChildren)
            throw new ValidatorException("Impossibile eliminare: il conto ha sotto-conti associati.");

        var hasItems = await session.Query<BudgetItem>()
            .AnyAsync(x => x.Account.Id == command.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (hasItems)
            throw new ValidatorException("Impossibile eliminare: il conto è utilizzato in voci di budget.");

        return await _accountService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
