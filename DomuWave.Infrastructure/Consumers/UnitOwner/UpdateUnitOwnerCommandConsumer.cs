using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateUnitOwnerCommandConsumer : InMemoryConsumerBase<UpdateUnitOwnerCommand, UnitOwnerReadDto>
{
    private readonly IUnitOwnerService      _unitOwnerService;
    private readonly IRealEstateUnitService _realEstateUnitService;
    private readonly IUserService           _userService;
    private readonly IAuthorizationClient   _authorizationClient;

    public UpdateUnitOwnerCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitOwnerService unitOwnerService,
        IRealEstateUnitService realEstateUnitService,
        IUserService userService,
        IAuthorizationClient authorizationClient) : base(sessionFactoryProvider)
    {
        _unitOwnerService      = unitOwnerService;
        _realEstateUnitService = realEstateUnitService;
        _userService           = userService;
        _authorizationClient   = authorizationClient;
    }

    protected override async Task<UnitOwnerReadDto> Consume(
        UpdateUnitOwnerCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _unitOwnerService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        existing.ApplyUpdate(command.Dto);

        var updated = await _unitOwnerService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Sync name back to the auth user record via Refit (email excluded: it's the login credential)
        if (existing.UserId > 0)
        {
            await _authorizationClient.UpdateUserAsync(
                currentUser.Token,
                (int)existing.UserId,
                new UpdateAuthUserRequest
                {
                    Name    = command.Dto.FirstName,
                    SurName = command.Dto.LastName,
                },
                cancellationToken).ConfigureAwait(false);
        }

        // Ricalcola DisplayName dell'unità solo se non è stato personalizzato
        var unitId = existing.Unit?.Id ?? 0;
        if (unitId > 0)
            await RefreshUnitDisplayName(unitId, currentUser, cancellationToken).ConfigureAwait(false);

        return updated.ToReadDto();
    }

    private async Task RefreshUnitDisplayName(int unitId, CPQ.Core.Memberships.IUser currentUser, CancellationToken ct)
    {
        var unit = await _realEstateUnitService.GetByIdAsync(unitId, currentUser, ct).ConfigureAwait(false);
        if (unit == null) return;

        var owners = await session.Query<UnitOwner>()
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
