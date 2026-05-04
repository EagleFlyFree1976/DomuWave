using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Building;
using DomuWave.Services.Dto.Building;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBuildingsByCondominiumCommandConsumer : InMemoryConsumerBase<GetBuildingsByCondominiumCommand, IList<BuildingReadDto>>
{
    private readonly IBuildingService _buildingService;
    private readonly IUserService     _userService;

    public GetBuildingsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBuildingService buildingService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _buildingService = buildingService;
        _userService     = userService;
    }

    protected override async Task<IList<BuildingReadDto>> Consume(
        GetBuildingsByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var buildings = await _buildingService
            .GetByCondominiumAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return buildings.Select(b => b.ToReadDto()).ToList();
    }
}
