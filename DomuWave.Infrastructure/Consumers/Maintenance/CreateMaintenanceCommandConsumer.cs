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

public class CreateMaintenanceCommandConsumer
    : InMemoryConsumerBase<CreateMaintenanceCommand, MaintenanceReadDto>
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ICondominiumService _condominiumService;
    private readonly ISupplierService    _supplierService;
    private readonly IUserService        _userService;

    public CreateMaintenanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IMaintenanceService maintenanceService,
        ICondominiumService condominiumService,
        ISupplierService supplierService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _maintenanceService = maintenanceService;
        _condominiumService = condominiumService;
        _supplierService    = supplierService;
        _userService        = userService;
    }

    protected override async Task<MaintenanceReadDto> Consume(
        CreateMaintenanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo della manutenzione è obbligatorio");

        var condominium = await _condominiumService.GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken).ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato");

        Services.Models.Supplier supplier = null;
        if (command.Dto.SupplierId.HasValue)
        {
            supplier = await _supplierService.GetByIdAsync(command.Dto.SupplierId.Value, currentUser, cancellationToken).ConfigureAwait(false);
            if (supplier == null)
                throw new NotFoundException("Fornitore non trovato");
        }

        var entity  = command.Dto.ToEntity(condominium, supplier);
        var created = await _maintenanceService.CreateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);

        return created.ToReadDto();
    }
}
