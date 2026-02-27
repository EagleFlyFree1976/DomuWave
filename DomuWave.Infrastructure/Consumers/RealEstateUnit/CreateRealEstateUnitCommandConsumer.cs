using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateRealEstateUnitCommandConsumer : InMemoryConsumerBase<CreateRealEstateUnitCommand, Models.RealEstateUnit>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService _userService;

    public CreateRealEstateUnitCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService = userService;
    }

    protected override async Task<Models.RealEstateUnit> Consume(
        CreateRealEstateUnitCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _realEstateUnitService
            .CreateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
