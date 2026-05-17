using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
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

        var previousEmail = existing.Email;
        existing.ApplyUpdate(command.Dto);
        var newEmail      = existing.Email;

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

        // Cascade email change to BillingGroup.ContactEmail and unsent CommunicationNotifications
        var unitId = existing.Unit?.Id ?? 0;
        if (unitId > 0 && !string.Equals(previousEmail, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var billingGroup = await session.Query<BillingGroup>()
                .Where(bg => bg.Units.Any(u => u.Id == unitId) && !bg.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (billingGroup != null)
            {
                billingGroup.ContactEmail = newEmail;
                billingGroup.Trace(currentUser);
                await session.UpdateAsync(billingGroup, cancellationToken).ConfigureAwait(false);

                // Update EmailAddress on Draft/Scheduled notifications sent to this billing group
                var pendingNotifs = await session.Query<CommunicationNotification>()
                    .Where(n => n.EmailAddress == previousEmail && n.Status <= 1 && !n.IsDeleted)
                    .ToListAsync(cancellationToken).ConfigureAwait(false);

                foreach (var notif in pendingNotifs)
                {
                    notif.EmailAddress = newEmail;
                    notif.Trace(currentUser);
                    await session.UpdateAsync(notif, cancellationToken).ConfigureAwait(false);
                }

                await session.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        // Ricalcola DisplayName dell'unità solo se non è stato personalizzato
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
