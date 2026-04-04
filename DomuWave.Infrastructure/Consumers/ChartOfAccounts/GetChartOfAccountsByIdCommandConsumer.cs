using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetChartOfAccountsByIdCommandConsumer
    : InMemoryConsumerBase<GetChartOfAccountsByIdCommand, ChartOfAccountsReadDto>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public GetChartOfAccountsByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IChartOfAccountsService accountService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService    = userService;
    }

    protected override async Task<ChartOfAccountsReadDto> Consume(
        GetChartOfAccountsByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var account = await _accountService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return account?.ToReadDto();
    }
}
