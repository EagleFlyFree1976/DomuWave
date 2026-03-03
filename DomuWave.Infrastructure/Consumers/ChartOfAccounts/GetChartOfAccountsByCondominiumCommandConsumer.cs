using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetChartOfAccountsByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetChartOfAccountsByCondominiumCommand, IList<ChartOfAccountsReadDto>>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public GetChartOfAccountsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IChartOfAccountsService accountService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService    = userService;
    }

    protected override async Task<IList<ChartOfAccountsReadDto>> Consume(
        GetChartOfAccountsByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var accounts = await _accountService
            .GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return accounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .Select(a => a.ToReadDto())
            .ToList();
    }
}
