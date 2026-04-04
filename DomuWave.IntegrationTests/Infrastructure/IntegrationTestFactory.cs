using CPQ.Core.ActionFilters;
using CPQ.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory that spins up the full DomuWave.Application pipeline
/// with authentication bypassed and domain services pointing to the test database.
///
/// Configuration is loaded from (in priority order):
///   1. Environment variables prefixed with DOMUWAVE_TEST_
///   2. appsettings.IntegrationTest.json  (copy to output dir)
///   3. Default appsettings.json from the Application project
///
/// Minimum required configuration (appsettings.IntegrationTest.json or env vars):
///   ConnectionStrings:sql-DomuWave  → test SQL Server connection string
///   IntegrationTest:TestUserId      → ID of an existing user in the test DB
///   IntegrationTest:TestTenantId    → GUID of an existing tenant in the test DB
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    public TestUserContext TestUser { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        // Overlay test-specific configuration
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var basePath = AppContext.BaseDirectory;
            config.AddJsonFile(
                Path.Combine(basePath, "appsettings.IntegrationTest.json"),
                optional: true,
                reloadOnChange: false);
            config.AddEnvironmentVariables("DOMUWAVE_TEST_");
        });

        builder.ConfigureTestServices(services =>
        {
            // ── Build test user from configuration ───────────────────────────
            var sp            = services.BuildServiceProvider();
            var cfg           = sp.GetRequiredService<IConfiguration>();
            var testSection   = cfg.GetSection("IntegrationTest");

            var userId   = testSection.GetValue<int>("TestUserId",       1);
            var tenantId = testSection.GetValue<Guid>("TestTenantId",    Guid.Empty);
            var fullName = testSection.GetValue<string>("TestUserFullName", "Integration Test User")!;
            var email    = testSection.GetValue<string>("TestUserEmail",    "integrationtest@domuwave.it")!;

            TestUser = TestUserContext.Create(userId, tenantId, fullName, email);

            // ── Inject test user into the pipeline before any filters run ────
            services.AddSingleton(TestUser);
            services.AddSingleton<IStartupFilter, TestAuthStartupFilter>();

            // ── Bypass TokenAuthorizeAttribute global filter ──────────────────
            // The filter is registered as Add<TokenAuthorizeAttribute>() which
            // results in a TypeFilterAttribute wrapping it.
            services.PostConfigure<MvcOptions>(options =>
            {
                var tokenFilter = options.Filters
                    .OfType<TypeFilterAttribute>()
                    .FirstOrDefault(f => f.ImplementationType?.Name.Contains("TokenAuthorize") == true);

                if (tokenFilter != null)
                    options.Filters.Remove(tokenFilter);

                // AllowAnonymous ensures no auth challenge is issued
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter());
            });

            // ── Mock CPQ.Core IUserService ────────────────────────────────────
            // Consumers call _userService.GetByIdAsync(command.CurrentUserId, ct)
            // to resolve the acting user. We return our TestUser for any ID.
            services.RemoveAll<IUserService>();
            var userServiceMock = new Mock<IUserService>();
            userServiceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestUser);
            userServiceMock
                .Setup(s => s.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestUser);
            services.AddScoped<IUserService>(_ => userServiceMock.Object);
        });
    }

    /// <summary>
    /// Creates an HttpClient pre-configured with the test tenant header.
    /// Every request will carry X-Tenant-Id so TenantHeaderFilter is satisfied.
    /// </summary>
    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        client.DefaultRequestHeaders.Add("X-Tenant-Id", TestUser.TenantId.ToString());
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TestUser.Token}");
        return client;
    }
}
