using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Building;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteBuildingCommandConsumer : InMemoryConsumerBase<DeleteBuildingCommand, bool>
{
    private readonly IBuildingService _buildingService;
    private readonly IUserService     _userService;

    public DeleteBuildingCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBuildingService buildingService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _buildingService = buildingService;
        _userService     = userService;
    }

    protected override async Task<bool> Consume(
        DeleteBuildingCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _buildingService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Edificio non trovato.");

        await _buildingService
            .DeleteAsync(entity.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
