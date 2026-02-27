using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetFeesTotalBalanceCommandConsumer : InMemoryConsumerBase<GetFeesTotalBalanceCommand, decimal>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService _userService;

    public GetFeesTotalBalanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumFeeService condominiumFeeService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumFeeService = condominiumFeeService;
        _userService = userService;
    }

    protected override async Task<decimal> Consume(
        GetFeesTotalBalanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumFeeService
            .GetTotalBalanceAsync(command.UserId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
