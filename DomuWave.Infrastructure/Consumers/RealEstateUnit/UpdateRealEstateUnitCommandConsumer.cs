using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateRealEstateUnitCommandConsumer : InMemoryConsumerBase<UpdateRealEstateUnitCommand, RealEstateUnitReadDto>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService           _userService;

    public UpdateRealEstateUnitCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
    }

    protected override async Task<RealEstateUnitReadDto> Consume(
        UpdateRealEstateUnitCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _realEstateUnitService
            .GetByIdAsync(command.UnitId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        existing.ApplyUpdate(command.Dto);

        var updated = await _realEstateUnitService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
