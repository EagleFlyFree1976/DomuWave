using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Maintenance;
using DomuWave.Services.Dto.Maintenance;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Maintenance;

public class UpdateMaintenanceCommandConsumer
    : InMemoryConsumerBase<UpdateMaintenanceCommand, MaintenanceReadDto>
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ISupplierService    _supplierService;
    private readonly IUserService        _userService;

    public UpdateMaintenanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMaintenanceService maintenanceService,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _maintenanceService = maintenanceService;
        _supplierService    = supplierService;
        _userService        = userService;
    }

    protected override async Task<MaintenanceReadDto> Consume(
        UpdateMaintenanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await _maintenanceService.GetByIdAsync(command.MaintenanceId, currentUser, cancellationToken).ConfigureAwait(false);
        if (entity == null) return null;

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo della manutenzione è obbligatorio");

        Services.Models.Supplier? supplier = null;
        if (command.Dto.SupplierId.HasValue)
        {
            supplier = await _supplierService.GetByIdAsync(command.Dto.SupplierId.Value, currentUser, cancellationToken).ConfigureAwait(false);
            if (supplier == null)
                throw new NotFoundException("Fornitore non trovato");
        }

        entity.ApplyUpdate(command.Dto, supplier);
        var updated = await _maintenanceService.UpdateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
