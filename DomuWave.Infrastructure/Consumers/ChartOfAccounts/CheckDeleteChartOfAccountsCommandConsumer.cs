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

public class CheckDeleteChartOfAccountsCommandConsumer
    : InMemoryConsumerBase<CheckDeleteChartOfAccountsCommand, CheckDeleteChartOfAccountsResult>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public CheckDeleteChartOfAccountsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IChartOfAccountsService accountService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService    = userService;
    }

    protected override async Task<CheckDeleteChartOfAccountsResult> Consume(
        CheckDeleteChartOfAccountsCommand command,
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

        // Classifica gli usi per stato del budget associato
        var usageStatuses = await session.Query<BudgetItem>()
            .Where(x => x.Account.Id == command.Id && !x.IsDeleted)
            .Select(x => x.Budget.Status.Id)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new CheckDeleteChartOfAccountsResult
        {
            HasChildren     = hasChildren,
            HasDraftUsages  = usageStatuses.Contains(BudgetStatus.Draft),
            HasLockedUsages = usageStatuses.Any(s => s != BudgetStatus.Draft),
        };
    }
}
