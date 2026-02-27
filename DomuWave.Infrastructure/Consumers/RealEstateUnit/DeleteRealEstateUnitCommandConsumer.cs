using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteRealEstateUnitCommandConsumer : InMemoryConsumerBase<DeleteRealEstateUnitCommand, bool>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService _userService;

    public DeleteRealEstateUnitCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        DeleteRealEstateUnitCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _realEstateUnitService
            .DeleteAsync(command.UnitId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
