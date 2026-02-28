using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetRealEstateUnitsByTypeCommandConsumer : InMemoryConsumerBase<GetRealEstateUnitsByTypeCommand, IList<RealEstateUnitReadDto>>
{
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService           _userService;

    public GetRealEstateUnitsByTypeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
    }

    protected override async Task<IList<RealEstateUnitReadDto>> Consume(
        GetRealEstateUnitsByTypeCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var units = await _realEstateUnitService
            .GetByTypeAsync(command.CondominiumId, command.UnitType, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return units.Select(u => u.ToReadDto()).ToList();
    }
}
