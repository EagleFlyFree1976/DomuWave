using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Maintenance;
using DomuWave.Services.Dto.Maintenance;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Maintenance;

public class GetMaintenanceByStatusCommandConsumer
    : InMemoryConsumerBase<GetMaintenanceByStatusCommand, IList<MaintenanceReadDto>>
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IUserService        _userService;

    public GetMaintenanceByStatusCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMaintenanceService maintenanceService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _maintenanceService = maintenanceService;
        _userService        = userService;
    }

    protected override async Task<IList<MaintenanceReadDto>> Consume(
        GetMaintenanceByStatusCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var list = await _maintenanceService.GetByStatusAsync(command.CondominiumId, command.Status, currentUser, cancellationToken).ConfigureAwait(false);
        return list.Select(x => x.ToReadDto()).ToList();
    }
}
