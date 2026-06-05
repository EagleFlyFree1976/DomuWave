using CPQ.Core;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Command.Auth;
using DomuWave.Services.Dto.Auth;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using DomuWave.Services.Settings;
using LicenseManager.Client.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Auth;

public class ConfirmRegistrationCommandConsumer
    : InMemoryConsumerBase<ConfirmRegistrationCommand, ConfirmRegistrationResultDto>
{
    private readonly IAuthorizationClient                    _authClient;
    private readonly IUserService                            _userService;
    private readonly ITenantService                          _tenantService;
    private readonly IUserTenantService                      _userTenantService;
    private readonly IPendingRegistrationService             _pendingService;
    private readonly IChartOfAccountsCategoryTemplateService _categoryTemplateService;
    private readonly IChartOfAccountsCategoryService         _categoryService;
    private readonly IChartOfAccountsTemplateSeedService     _coaTemplateSeed;
    private readonly ILicenseManagerClient                   _licenseClient;
    private readonly IMediator                               _mediator;
    private readonly DomuWaveSettings                        _settings;
    private readonly ILogger<ConfirmRegistrationCommandConsumer> _logger;

    public ConfirmRegistrationCommandConsumer(
        ISessionFactoryProvider                  sessionFactoryProvider,
        IAuthorizationClient                     authClient,
        IUserService                             userService,
        ITenantService                           tenantService,
        IUserTenantService                       userTenantService,
        IPendingRegistrationService              pendingService,
        IChartOfAccountsCategoryTemplateService  categoryTemplateService,
        IChartOfAccountsCategoryService          categoryService,
        IChartOfAccountsTemplateSeedService      coaTemplateSeed,
        ILicenseManagerClient                    licenseClient,
        IMediator                                mediator,
        IOptionsMonitor<DomuWaveSettings>        settings,
        ILogger<ConfirmRegistrationCommandConsumer> logger) : base(sessionFactoryProvider)
    {
        _authClient              = authClient;
        _userService             = userService;
        _tenantService           = tenantService;
        _userTenantService       = userTenantService;
        _pendingService          = pendingService;
        _categoryTemplateService = categoryTemplateService;
        _categoryService         = categoryService;
        _coaTemplateSeed         = coaTemplateSeed;
        _licenseClient           = licenseClient;
        _mediator                = mediator;
        _settings                = settings.CurrentValue;
        _logger                  = logger;
    }

    protected override async Task<ConfirmRegistrationResultDto> Consume(
        ConfirmRegistrationCommand command,
        IMediationContext          mediationContext,
        CancellationToken          cancellationToken)
    {
        // 1. Carica il record di staging tramite il token di verifica
        var pending = await _pendingService.GetByTokenAsync(command.Token, cancellationToken)
            .ConfigureAwait(false);

        if (pending == null)
            throw new NotFoundException("Link di verifica non valido.");

        if (pending.Status != PendingRegistrationStatus.Pending)
            throw new ValidatorException("La registrazione è già stata confermata o è scaduta.");

        if (pending.VerificationExpiresAt is null || pending.VerificationExpiresAt < DateTime.UtcNow)
        {
            pending.Status = PendingRegistrationStatus.Expired;
            await _pendingService.UpdateAsync(pending, cancellationToken).ConfigureAwait(false);
            throw new ValidatorException("Il link di verifica è scaduto. Effettua una nuova registrazione.");
        }

        // 2. Verifica esistenza utente per email
        var existing = await _authClient.SearchUsersAsync(
            CommonKeys.SystemUserToken,
            search:   pending.Email,
            isActive: null,
            roles:    null,
            cancellationToken).ConfigureAwait(false);

        var match = existing?.FirstOrDefault(u =>
            string.Equals(u.Email, pending.Email, StringComparison.OrdinalIgnoreCase));

        CPQ.Core.DTO.UserDto authUser;
        var createdNewAuthUser = false;   // true solo se abbiamo CREATO l'utente (per compensazione)

        var isCondomino = match != null && string.Equals(match.RoleCode, "Condomino", StringComparison.OrdinalIgnoreCase);

        // Un account amministratore ATTIVO con questa email blocca la registrazione.
        // Se invece è disattivato (residuo di un tentativo precedente fallito) lo riusiamo.
        if (match != null && !isCondomino && match.IsActive)
            throw new ValidatorException("Esiste già un account amministratore con questo indirizzo email.");

        if (match != null)
        {
            // Promuovi/riattiva l'utente esistente come Amministratore (AMS)
            await _authClient.UpdateUserAsync(
                CommonKeys.SystemUserToken,
                match.Id,
                new UpdateAuthUserRequest
                {
                    Name     = match.Name,
                    SurName  = match.LastName,
                    Email    = match.Email,
                    IsActive = true,
                    RoleCode = "AMS",
                },
                cancellationToken).ConfigureAwait(false);

            authUser = await _authClient.GetUserByIdAsync(
                CommonKeys.SystemUserToken, match.Id, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            // Crea nuovo utente; la password è già cifrata con EncryptString nel pending record
            authUser = await _authClient.CreateUserAsync(
                CommonKeys.SystemUserToken,
                new CreateAuthUserRequest
                {
                    Email      = pending.Email,
                    Name       = pending.Email,
                    Password   = pending.PasswordHash,
                    SurName    = string.Empty,
                    RoleCode   = "AMS",
                    ModuleCode = "DomuWeb",
                },
                cancellationToken).ConfigureAwait(false);
            createdNewAuthUser = true;
        }

        // Autore delle creazioni DomuWave in fase di attivazione: l'utente di SISTEMA,
        // recuperato tramite il Code configurato in DomuWaveSettings.SystemUserCode.
        // L'utente appena creato in Auth non è un autore valido lato DomuWave (CreatedById=0,
        // Code=null, e potrebbe non essere ancora materializzato sul DB DomuWave).
        if (string.IsNullOrWhiteSpace(_settings.SystemUserCode))
            throw new ValidatorException("SystemUserCode non configurato: impossibile completare la registrazione.");

        var domainUser = await _userService.GetByCodeAsync(_settings.SystemUserCode, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Utente di sistema '{_settings.SystemUserCode}' non trovato.");

        // Da qui in poi tutte le operazioni DomuWave devono essere atomiche: se una fallisce,
        // il commit del NHibernateMiddleware fa rollback di tenant/condominio/pending.
        // L'utente Auth è su un DB separato: in caso di errore lo eliminiamo per compensazione.
        try
        {

        // 3. Crea tenant con il GUID del pending record come Id (same-id guarantee).
        //    Il Code ha un vincolo UNIQUE: genero un codice base dal nome e, in caso di collisione,
        //    aggiungo un suffisso numerico incrementale finché non ne trovo uno libero (univocità garantita).
        var code = await GenerateUniqueTenantCodeAsync(pending.TenantName, cancellationToken)
            .ConfigureAwait(false);

        var tenantEntity = new Models.Tenant
        {
            Id       = pending.Id,
            Name     = pending.TenantName,
            Code     = code,
            IsActive = true,
        };
        var createdTenant = await _tenantService.CreateAsync(tenantEntity, domainUser, cancellationToken)
            .ConfigureAwait(false);

        // 4. Seed categorie piano dei conti dal template
        var templates = await _categoryTemplateService
            .GetActiveAsync(domainUser, cancellationToken)
            .ConfigureAwait(false);

        foreach (var template in templates)
        {
            var category = template.ToCategory(createdTenant);
            await _categoryService.CreateAsync(category, domainUser, cancellationToken)
                .ConfigureAwait(false);
        }

        await _coaTemplateSeed.SeedAsync(createdTenant, cancellationToken).ConfigureAwait(false);

        // 5. Associa utente al tenant
        var userTenant = new UserTenant
        {
            UserId    = authUser.Id,
            Tenant    = createdTenant,
            IsDefault = true,
            IsActive  = true,
        };
        userTenant.Trace(domainUser);
        await _userTenantService.CreateAsync(userTenant, domainUser, cancellationToken)
            .ConfigureAwait(false);

        // 6. Notifica LicenseManager (fire-and-forget)
        try
        {
            var result = await _licenseClient
                .NotifyTenantCreatedAsync(createdTenant.Id, pending.Email, createdTenant.Name, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Success)
                _logger.LogWarning("LicenseManager: notifica tenant {TenantId} fallita: {Error}", createdTenant.Id, result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LicenseManager non raggiungibile durante la conferma registrazione per tenant {TenantId}.", createdTenant.Id);
        }

        // 7. Marca pending come confermato e flush PRIMA di creare il condominio,
        //    così il flush del pending non aggancia in cascade l'address del condominio.
        pending.Status      = PendingRegistrationStatus.Confirmed;
        pending.ConfirmedAt = DateTime.UtcNow;
        await _pendingService.UpdateAsync(pending, cancellationToken).ConfigureAwait(false);

        // 8. Crea il primo condominio (se i dati sono stati forniti nello step 3).
        //    Se fallisce, l'eccezione si propaga e fa rollback di TUTTO (atomicità).
        if (!string.IsNullOrWhiteSpace(pending.CondominiumName))
        {
            var condoDto = new DomuWave.Services.Dto.Condominium.CreateCondominiumDto
            {
                Name = pending.CondominiumName!,
                Code = string.IsNullOrWhiteSpace(pending.CondominiumCode)
                    ? pending.CondominiumName!.Trim()
                    : pending.CondominiumCode!,
                // Campi NOT NULL con default sensati (l'utente li rifinirà dopo)
                InstallmentFrequency = "Monthly",
                InstallmentDueDay    = 1,
                Address = new DomuWave.Services.Dto.Condominium.CondominiumAddressDto
                {
                    Street       = string.Empty,
                    StreetNumber = string.Empty,
                    PostalCode   = pending.CondominiumZip  ?? string.Empty,
                    City         = pending.CondominiumCity ?? string.Empty,
                    Province     = string.Empty,
                    Country      = "IT",
                },
            };
            await _mediator.GetResponse(
                new DomuWave.Services.Command.Condominium.CreateCondominiumCommand(domainUser.Id, createdTenant.Id, condoDto),
                cancellationToken).ConfigureAwait(false);
        }

        // Forza il flush per far emergere qui eventuali errori DB (es. constraint),
        // prima di restituire — così la compensazione dell'utente Auth può scattare.
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return new ConfirmRegistrationResultDto
        {
            Token      = authUser.Token ?? string.Empty,
            UserId     = authUser.Id,
            Email      = authUser.Email ?? pending.Email,
            TenantId   = createdTenant.Id,
            TenantName = createdTenant.Name,
        };

        }
        catch (Exception ex)
        {
            // Compensazione: se abbiamo creato l'utente Auth ma qualcosa è poi fallito,
            // ne facciamo la cancellazione LOGICA (DeleteUserAsync → IsActive=false): l'utente è su
            // un DB separato non coperto dal rollback NHibernate del DB DomuWave.
            _logger.LogError(ex, "Conferma registrazione fallita per {Email}: rollback in corso.", pending.Email);

            if (createdNewAuthUser)
            {
                try
                {
                    await _authClient.DeleteUserAsync(CommonKeys.SystemUserToken, authUser.Id, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception delEx)
                {
                    _logger.LogError(delEx, "Compensazione: impossibile disattivare l'utente Auth {UserId}.", authUser.Id);
                }
            }

            throw;   // rilancia: il NHibernateMiddleware farà rollback del DB DomuWave
        }
    }

    /// <summary>
    /// Genera un codice tenant univoco partendo dal nome. Normalizza il nome (A-Z, 0-9, _),
    /// poi verifica l'esistenza nel DB e, in caso di collisione, aggiunge un suffisso numerico
    /// incrementale (_2, _3, ...) finché non trova un codice libero. Univocità garantita.
    /// </summary>
    private async Task<string> GenerateUniqueTenantCodeAsync(string tenantName, CancellationToken ct)
    {
        const int maxLen = 50;

        var baseCode = System.Text.RegularExpressions.Regex
            .Replace(tenantName.ToUpperInvariant(), @"[^A-Z0-9_]", "_");
        baseCode = System.Text.RegularExpressions.Regex.Replace(baseCode, @"_+", "_").Trim('_');
        if (string.IsNullOrEmpty(baseCode)) baseCode = "TENANT";
        if (baseCode.Length > maxLen) baseCode = baseCode[..maxLen];

        var candidate = baseCode;
        var suffix    = 1;

        while (await session.Query<Models.Tenant>()
                   .AnyAsync(t => t.Code == candidate, ct)
                   .ConfigureAwait(false))
        {
            suffix++;
            var tag = $"_{suffix}";
            var trimmed = baseCode.Length + tag.Length > maxLen
                ? baseCode[..(maxLen - tag.Length)]
                : baseCode;
            candidate = trimmed + tag;
        }

        return candidate;
    }
}
