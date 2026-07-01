using Auth.Services.Command;
using Auth.Services.Interfaces;
using Auth.Services.Orchestators;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using AuthUser = Auth.Services.Models.User;

namespace DomuWave.Services.Implementations;

public class OccupantUserProvisioningService : BaseService, IOccupantUserProvisioningService
{
    private readonly AuthOrchestator  _authOrchestator;
    private readonly IAuthUserService _authUserService;
    private readonly IConfiguration   _configuration;
    private readonly ILogger<OccupantUserProvisioningService> _logger;

    private const string CondominoRoleCode = "Condomino";
    private const string DomuWebModuleCode  = "DomuWeb";
    private const string DefaultPlaceholderDomain = "placeholder.domuwave.local";

    public OccupantUserProvisioningService(
        ISessionFactoryProvider sessionFactoryProvider,
        ICacheManager           cache,
        AuthOrchestator         authOrchestator,
        IAuthUserService        authUserService,
        IConfiguration          configuration,
        ILogger<OccupantUserProvisioningService> logger) : base(sessionFactoryProvider, cache)
    {
        _authOrchestator = authOrchestator;
        _authUserService = authUserService;
        _configuration   = configuration;
        _logger          = logger;
    }

    public override string CacheRegion => "OccupantProvisioning";

    public async Task<OccupantProvisioningResult> EnsureUserAsync(
        string? email,
        string? firstName,
        string? lastName,
        Tenant tenant,
        IUser currentUser,
        CancellationToken cancellationToken)
    {
        var isPlaceholder = string.IsNullOrWhiteSpace(email);
        var effectiveEmail = isPlaceholder ? GeneratePlaceholderEmail() : email!.Trim();

        // 1. Trova un utente auth esistente con questa email (Name == email).
        //    Gli utenti auth vivono sulla session factory "AUTH": si interrogano
        //    tramite IAuthUserService, non sulla session di DomuWave.
        var existing = _authUserService.GetQueryable()
            .Where(u => u.Name == effectiveEmail && !u.IsDeleted)
            .FirstOrDefault();

        long userId;
        if (existing != null)
        {
            userId = existing.Id;
        }
        else
        {
            // 2. Crea l'utente auth (password casuale: l'accesso avverrà tramite invito/reset).
            var created = await _authOrchestator.CreateUser(new CreateUser
            {
                Email      = effectiveEmail,
                Name       = firstName ?? string.Empty,
                SurName    = lastName ?? string.Empty,
                Password   = Guid.NewGuid().ToString("N"),
                RoleCode   = CondominoRoleCode,
                ModuleCode = DomuWebModuleCode,
            }, cancellationToken).ConfigureAwait(false);

            userId = created.Id;
        }

        // 3. Associa l'utente al tenant del condominio (se non già associato).
        await EnsureTenantLinkAsync((int)userId, tenant, currentUser, cancellationToken).ConfigureAwait(false);

        // 4. Invito/reset SOLO se l'email è reale (non placeholder).
        if (!isPlaceholder)
        {
            try
            {
                await _authOrchestator
                    .GeneratePasswordResetAsync(effectiveEmail, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // L'invito non deve far fallire la creazione dell'occupante.
                _logger.LogWarning(ex, "Invito password non inviato per {email}", effectiveEmail);
            }
        }

        return new OccupantProvisioningResult(userId, isPlaceholder);
    }

    public async Task ChangeEmailAndInviteAsync(
        long userId,
        string newEmail,
        IUser currentUser,
        CancellationToken cancellationToken)
    {
        var user = await _authUserService.GetByIdAsync((int)userId, cancellationToken).ConfigureAwait(false);
        if (user == null) return;

        await _authUserService.UpdateUser((int)userId, new UpdateUser
        {
            Name     = user.FirstName,
            SurName  = user.LastName,
            Email    = newEmail.Trim(),
            IsActive = user.IsActive,
            RoleCode = user.Role?.Code,
        }, cancellationToken).ConfigureAwait(false);

        await _authOrchestator
            .GeneratePasswordResetAsync(newEmail.Trim(), cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task EnsureTenantLinkAsync(int userId, Tenant tenant, IUser currentUser, CancellationToken ct)
    {
        if (tenant == null) return;

        var alreadyLinked = await session.Query<UserTenant>()
            .AnyAsync(ut => ut.UserId == userId && ut.Tenant.Id == tenant.Id && !ut.IsDeleted, ct)
            .ConfigureAwait(false);
        if (alreadyLinked) return;

        var link = new UserTenant
        {
            UserId    = userId,
            Tenant    = tenant,
            IsDefault = false,
            IsActive  = true,
            RoleCode  = "Condomino",
        };
        link.Trace(currentUser);
        await session.SaveAsync(link, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
    }

    private string GeneratePlaceholderEmail()
    {
        var domain = _configuration["DomuWave:PlaceholderEmailDomain"];
        if (string.IsNullOrWhiteSpace(domain)) domain = DefaultPlaceholderDomain;
        return $"noemail+{Guid.NewGuid():N}@{domain.Trim()}";
    }
}
