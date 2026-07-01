using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitTenant;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateUnitTenantCommandConsumer : InMemoryConsumerBase<CreateUnitTenantCommand, UnitTenantReadDto>
{
    private readonly IUnitTenantService      _unitTenantService;
    private readonly IRealEstateUnitService  _realEstateUnitService;
    private readonly IUserService            _userService;
    private readonly IOccupantUserProvisioningService _provisioning;

    public CreateUnitTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitTenantService unitTenantService,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService,
        IOccupantUserProvisioningService provisioning) : base(sessionFactoryProvider)
    {
        _unitTenantService     = unitTenantService;
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
        _provisioning          = provisioning;
    }

    protected override async Task<UnitTenantReadDto> Consume(
        CreateUnitTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var unit = await _realEstateUnitService
            .GetByIdAsync(command.Dto.UnitId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (unit == null)
            throw new NotFoundException("Unità immobiliare non trovata");

        var entity = command.Dto.ToEntity(unit, unit.Tenant);

        // Se l'accesso è abilitato e non è già stato collegato un utente esistente,
        // crea (o riusa) l'utente di piattaforma per questo occupante.
        if (entity.IsAccessEnabled && entity.UserId <= 0)
        {
            var result = await _provisioning
                .EnsureUserAsync(command.Dto.Email, command.Dto.FirstName, command.Dto.LastName,
                                 unit.Tenant, currentUser, cancellationToken)
                .ConfigureAwait(false);
            entity.UserId = result.UserId;
        }

        var created = await _unitTenantService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
