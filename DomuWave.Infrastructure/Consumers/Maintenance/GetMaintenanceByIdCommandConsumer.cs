using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Maintenance;
using DomuWave.Services.Dto.Maintenance;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Maintenance;

public class GetMaintenanceByIdCommandConsumer
    : InMemoryConsumerBase<GetMaintenanceByIdCommand, MaintenanceReadDto>
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IUserService        _userService;

    public GetMaintenanceByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMaintenanceService maintenanceService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _maintenanceService = maintenanceService;
        _userService        = userService;
    }

    protected override async Task<MaintenanceReadDto> Consume(
        GetMaintenanceByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity = await _maintenanceService.GetByIdAsync(command.MaintenanceId, currentUser, cancellationToken).ConfigureAwait(false);
        return entity?.ToReadDto();
    }
}
