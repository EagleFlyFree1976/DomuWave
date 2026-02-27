using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetFeesByUnitCommandConsumer : InMemoryConsumerBase<GetFeesByUnitCommand, IList<CondominiumFee>>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService _userService;

    public GetFeesByUnitCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumFeeService condominiumFeeService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumFeeService = condominiumFeeService;
        _userService = userService;
    }

    protected override async Task<IList<CondominiumFee>> Consume(
        GetFeesByUnitCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumFeeService
            .GetByUnitIdAsync(command.UnitId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
