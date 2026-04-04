namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// IStartupFilter that registers TestAuthMiddleware at the very beginning
/// of the pipeline — before routing, auth, and any action filters.
/// </summary>
public class TestAuthStartupFilter(TestUserContext testUser) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // Must come first to ensure HttpContext.Items is populated early
            app.UseMiddleware<TestAuthMiddleware>(testUser);
            next(app);
        };
    }
}
