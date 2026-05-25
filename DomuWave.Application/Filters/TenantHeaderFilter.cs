using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SimpleMediator.Core;

namespace DomuWave.Application.Filters;

public class TenantHeaderFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-Tenant-Id";

    private readonly IMediator          _mediator;
    private readonly IUserService       _userService;
    private readonly ITenantAccessCache _accessCache;

    public TenantHeaderFilter(IMediator mediator, IUserService userService, ITenantAccessCache accessCache)
    {
        _mediator    = mediator;
        _userService = userService;
        _accessCache = accessCache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out StringValues headerValue)
            || string.IsNullOrWhiteSpace(headerValue))
        {
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }

        var tenantId = headerValue.ToString();
        context.HttpContext.Items["TenantId"] = tenantId;

        IUser user = context.HttpContext.User as IUser;

        if (user == null && context.HttpContext.Request.Headers.TryGetValue("X-Auth-Token", out StringValues tokenValue))
        {
            var token = tokenValue.ToString();
            if (!string.IsNullOrEmpty(token))
                user = await _userService.GetByTokenAsync(token, CancellationToken.None).ConfigureAwait(false);
        }

        if (user == null)
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");

        if (user.IsSystemUser)
        {
            await next();
            return;
        }
        if (!Guid.TryParse(tenantId, out var tenantGuid))
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");

        if (_accessCache.TryGetAccess(user.Id, tenantGuid, out bool cached))
        {
            if (!cached)
                throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");

            context.HttpContext.Items["TenantId"] = tenantGuid;
            await next();
            return;
        }

        // Cache miss: verifica sul DB
        var tenant = await _mediator
            .GetResponse(new GetTenantByIdCommand(user.Id, tenantGuid), CancellationToken.None)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            _accessCache.SetAccess(user.Id, tenantGuid, false);
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }

        bool canAccess = await _mediator
            .GetResponse(new CanUserAccessToTenantCommand(user.Id, tenantGuid), CancellationToken.None)
            .ConfigureAwait(false);

        _accessCache.SetAccess(user.Id, tenantGuid, canAccess);

        if (!canAccess)
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");

        context.HttpContext.Items["TenantId"] = tenantGuid;

        await next();
    }
}