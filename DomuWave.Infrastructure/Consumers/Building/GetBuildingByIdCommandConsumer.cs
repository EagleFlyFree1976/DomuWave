using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Building;
using DomuWave.Services.Dto.Building;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBuildingByIdCommandConsumer : InMemoryConsumerBase<GetBuildingByIdCommand, BuildingReadDto>
{
    private readonly IBuildingService _buildingService;
    private readonly IUserService     _userService;

    public GetBuildingByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBuildingService buildingService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _buildingService = buildingService;
        _userService     = userService;
    }

    protected override async Task<BuildingReadDto> Consume(
        GetBuildingByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var building = await _buildingService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return building?.ToReadDto();
    }
}
