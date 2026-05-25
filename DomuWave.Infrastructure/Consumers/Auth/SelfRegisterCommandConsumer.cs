using CPQ.Core;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Security;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Command.Auth;
using DomuWave.Services.Dto.Auth;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Auth;

public class SelfRegisterCommandConsumer
    : InMemoryConsumerBase<SelfRegisterCommand, SelfRegisterResultDto>
{
    private readonly IAuthorizationClient          _authClient;
    private readonly IPendingRegistrationService   _pendingService;

    public SelfRegisterCommandConsumer(
        ISessionFactoryProvider        sessionFactoryProvider,
        IAuthorizationClient           authClient,
        IPendingRegistrationService    pendingService) : base(sessionFactoryProvider)
    {
        _authClient     = authClient;
        _pendingService = pendingService;
    }

    protected override async Task<SelfRegisterResultDto> Consume(
        SelfRegisterCommand command,
        IMediationContext   mediationContext,
        CancellationToken   cancellationToken)
    {
        // 1. Controlla se esiste già un utente con questa email
        var existing = await _authClient.SearchUsersAsync(
            CommonKeys.SystemUserToken,
            search:   command.Email,
            isActive: null,
            roles:    null,
            cancellationToken).ConfigureAwait(false);

        var match = existing?.FirstOrDefault(u =>
            string.Equals(u.Email, command.Email, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            var roleCode = match.RoleCode ?? string.Empty;

            if (!string.Equals(roleCode, "Condomino", StringComparison.OrdinalIgnoreCase))
                throw new ValidatorException("Esiste già un account amministratore con questo indirizzo email.");

            // Ruolo Condomino → promuovibile ad Amministratore AMS (upgrade avviene in ConfirmRegistration)
            // Qui non agiamo ancora: salviamo comunque il pending record e gestiremo l'upgrade al confirm.
        }

        // 2. Annulla eventuali pending precedenti per la stessa email
        var previous = await _pendingService.GetPendingByEmailAsync(command.Email, cancellationToken)
            .ConfigureAwait(false);

        if (previous != null)
        {
            previous.Status = PendingRegistrationStatus.Expired;
            await _pendingService.UpdateAsync(previous, cancellationToken).ConfigureAwait(false);
        }

        // 3. Salva il record di staging
        //    La password viene cifrata con lo stesso algoritmo di base_users (EncryptString)
        //    così ConfirmRegistration può passarla direttamente a CreateUserAsync
        var pending = new PendingRegistration
        {
            Id           = Guid.NewGuid(),
            Email        = command.Email.Trim().ToLowerInvariant(),
            PasswordHash = command.Password.EncryptString(),
            TenantName   = command.TenantName.Trim(),
            Status       = PendingRegistrationStatus.Pending,
            CreatedAt    = DateTime.UtcNow,
            ExpiresAt    = DateTime.UtcNow.AddDays(7),
        };

        await _pendingService.CreateAsync(pending, cancellationToken).ConfigureAwait(false);

        return new SelfRegisterResultDto
        {
            RegistrationId = pending.Id,
            Email          = pending.Email,
            TenantName     = pending.TenantName,
        };
    }
}
