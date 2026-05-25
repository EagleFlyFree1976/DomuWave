using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Clients;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using LicenseManager.Client.Cache;
using LicenseManager.Client.Models;
using LicenseManager.Client.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Implementations;

public class MenuService : BaseService, IMenuService
{
    protected IAuthorizationClient _authorizationClient;
    protected IMediator _mediator;
    protected ILicenseManagerClient _licenseClient;

    public override string CacheRegion
    {
        get { return "Menu"; }
    }

    public MenuService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, IAuthorizationClient authorizationClient, IMediator mediator, ILicenseManagerClient licenseClient) : base(sessionFactoryProvider, cache)
    {
        _authorizationClient = authorizationClient;
        _mediator = mediator;
        _licenseClient = licenseClient;
    }

    public async Task<IList<MenuItem>> GetAllMenuItems(IUser currentUser, Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var allMenuItems = await session.Query<MenuItem>().OrderBy(k => k.OrderKey).ToListAsync(cancellationToken).ConfigureAwait(false);

        var allUserAuthorization = await _authorizationClient
            .GetAllUserAuthorizations(currentUser.Token, currentUser.Id, cancellationToken).ConfigureAwait(false);

        var authCodes = allUserAuthorization.Select(a => a.AuthCode).ToHashSet();

        // Carica i token del tenant specifico (cache-first, zero rete se già in cache)
        TenantTokenCache? tokenCache = tenantId.HasValue
            ? await _licenseClient.GetTokensAsync(tenantId.Value, cancellationToken).ConfigureAwait(false)
            : null;

        var authorizedMenuItems = allMenuItems
            .Where(item => authCodes.Contains(item.AuthorizationCode) || string.IsNullOrEmpty(item.AuthorizationCode))
            .Where(item => HasRequiredFeatures(item, tokenCache))
            .ToList();

        //authorizedMenuItems = authorizedMenuItems.Where(item => item.MatchesVisibility(tenantId)).ToList();
        return authorizedMenuItems;
    }

    private static bool HasRequiredFeatures(MenuItem item, TenantTokenCache? tokenCache)
    {
        if (string.IsNullOrWhiteSpace(item.Features))
            return true;

        // Nessun tenant (es. SuperAdmin senza tenant) → mostra sempre
        if (tokenCache is null)
            return true;

        var required = item.Features
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return required.All(code =>
            tokenCache.Tokens.TryGetValue(code, out var token) && token.IsActive);
    }



}