using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// IStartupFilter that registers TestAuthMiddleware at the very beginning
/// of the pipeline — before routing, auth, and any action filters.
/// </summary>
public class TestAuthStartupFilter(IntegrationTestFactory factory) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<TestAuthMiddleware>(factory);
            next(app);
        };
    }
}
