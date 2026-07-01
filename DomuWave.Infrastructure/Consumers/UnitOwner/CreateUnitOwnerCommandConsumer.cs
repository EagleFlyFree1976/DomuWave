using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateUnitOwnerCommandConsumer : InMemoryConsumerBase<CreateUnitOwnerCommand, UnitOwnerReadDto>
{
    private readonly IUnitOwnerService       _unitOwnerService;
    private readonly IRealEstateUnitService  _realEstateUnitService;
    private readonly IUserService            _userService;
    private readonly IOccupantUserProvisioningService _provisioning;

    public CreateUnitOwnerCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitOwnerService unitOwnerService,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService,
        IOccupantUserProvisioningService provisioning) : base(sessionFactoryProvider)
    {
        _unitOwnerService      = unitOwnerService;
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
        _provisioning          = provisioning;
    }

    protected override async Task<UnitOwnerReadDto> Consume(
        CreateUnitOwnerCommand command,
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

        var created = await _unitOwnerService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Ricalcola DisplayName dell'unità dai proprietari attivi (se non già personalizzato)
        await RefreshUnitDisplayName(unit.Id, currentUser, cancellationToken).ConfigureAwait(false);

        return created.ToReadDto();
    }

    private async Task RefreshUnitDisplayName(int unitId, CPQ.Core.Memberships.IUser currentUser, CancellationToken ct)
    {
        var unit = await _realEstateUnitService.GetByIdAsync(unitId, currentUser, ct).ConfigureAwait(false);
        if (unit == null) return;

        var owners = await session.Query<DomuWave.Services.Models.UnitOwner>()
            .Where(o => o.Unit.Id == unitId && o.IsActive && !o.IsDeleted)
            .OrderBy(o => o.LastName)
            .ToListAsync(ct).ConfigureAwait(false);

        if (owners.Any())
        {
            var displayName = string.Join(" / ", owners
                .Select(o => string.IsNullOrWhiteSpace(o.LastName) ? o.FirstName : o.LastName)
                .Where(n => !string.IsNullOrWhiteSpace(n)));
            unit.RefreshDisplayName(displayName);
        }

        await _realEstateUnitService.UpdateAsync(unit, currentUser, ct).ConfigureAwait(false);
    }
}
