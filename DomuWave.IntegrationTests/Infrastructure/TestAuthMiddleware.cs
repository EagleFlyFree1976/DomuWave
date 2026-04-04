namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// Middleware injected at the very start of the ASP.NET Core pipeline in integration tests.
/// Pre-populates HttpContext.Items so that:
///   - TenantHeaderFilter finds "TenantId" already set and skips the real tenant validation.
///   - OxCoreTokenAuthorizeControllerBase finds "CurrentUser" and resolves CurrentUser.Id.
/// </summary>
public class TestAuthMiddleware(
    RequestDelegate  next,
    TestUserContext  testUser)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Inject TenantId — bypasses TenantHeaderFilter's validation block
        context.Items["TenantId"]    = testUser.TenantId;

        // 2. Inject the user so base controller resolves CurrentUser
        context.Items["CurrentUser"] = testUser;

        // 3. Also set as ClaimsPrincipal for frameworks that read HttpContext.User
        context.User = new System.Security.Claims.ClaimsPrincipal(testUser as System.Security.Principal.IIdentity
                           ?? new System.Security.Claims.ClaimsIdentity("IntegrationTest"));

        await next(context);
    }
}
