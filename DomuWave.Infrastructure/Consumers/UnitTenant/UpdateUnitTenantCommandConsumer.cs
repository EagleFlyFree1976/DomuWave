using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitTenant;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateUnitTenantCommandConsumer : InMemoryConsumerBase<UpdateUnitTenantCommand, UnitTenantReadDto>
{
    private readonly IUnitTenantService _unitTenantService;
    private readonly IUserService       _userService;
    private readonly IOccupantUserProvisioningService _provisioning;

    public UpdateUnitTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitTenantService unitTenantService,
        IUserService userService,
        IOccupantUserProvisioningService provisioning) : base(sessionFactoryProvider)
    {
        _unitTenantService = unitTenantService;
        _userService       = userService;
        _provisioning      = provisioning;
    }

    protected override async Task<UnitTenantReadDto> Consume(
        UpdateUnitTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _unitTenantService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        var previousEmail = existing.Email;
        var hadUser       = existing.UserId > 0;
        existing.ApplyUpdate(command.Dto);
        var newEmail      = existing.Email;

        // Provisiona ora l'account se l'accesso è abilitato, non c'è ancora un
        // utente collegato ed è disponibile un'email.
        if (existing.IsAccessEnabled && existing.UserId <= 0 && !string.IsNullOrWhiteSpace(newEmail))
        {
            var unitTenant = existing.Unit?.Tenant ?? existing.Tenant;
            var result = await _provisioning
                .EnsureUserAsync(newEmail, existing.FirstName, existing.LastName,
                                 unitTenant, currentUser, cancellationToken)
                .ConfigureAwait(false);
            existing.UserId = result.UserId;
        }

        var updated = await _unitTenantService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // L'admin ha aggiornato l'email reale: aggiorna l'email auth e reinvia
        // l'invito/reset (vale anche per gli utenti creati con email placeholder).
        if (hadUser && existing.UserId > 0 && !string.IsNullOrWhiteSpace(newEmail)
            && !string.Equals(previousEmail, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            await _provisioning
                .ChangeEmailAndInviteAsync(existing.UserId, newEmail, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        return updated.ToReadDto();
    }
}
