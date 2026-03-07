using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Maintenance;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Maintenance;

public class DeleteMaintenanceCommandConsumer
    : InMemoryConsumerBase<DeleteMaintenanceCommand, bool>
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IUserService        _userService;

    public DeleteMaintenanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMaintenanceService maintenanceService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _maintenanceService = maintenanceService;
        _userService        = userService;
    }

    protected override async Task<bool> Consume(
        DeleteMaintenanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return await _maintenanceService.DeleteAsync(command.MaintenanceId, currentUser, cancellationToken).ConfigureAwait(false);
    }
}
