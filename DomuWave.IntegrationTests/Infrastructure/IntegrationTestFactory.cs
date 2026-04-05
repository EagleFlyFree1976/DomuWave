using CPQ.Core.ActionFilters;
using CPQ.Core.Memberships;
using CPQ.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace DomuWave.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory that spins up the full DomuWave.Application pipeline
/// with authentication bypassed and domain services pointing to the test database.
///
/// Supports multiple test roles: DMW_SU, PRT_ADM, AMS, CLB, Condomino.
/// Use CreateClientAs(role) to get a client authenticated as a specific role.
/// The default client (CreateAuthenticatedClient) uses PRT_ADM.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    /// <summary>All test users keyed by role, loaded at construction time.</summary>
    public IReadOnlyDictionary<TestRole, TestUserContext> Users { get; }

    /// <summary>Default test user (PRT_ADM).</summary>
    public TestUserContext TestUser => Users[TestRole.DMW_SU];

    public Guid TenantId { get; }

    public IntegrationTestFactory()
    {
        var cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.IntegrationTest.json", optional: true)
            .AddEnvironmentVariables("DOMUWAVE_TEST_")
            .Build();

        TenantId = cfg.GetValue<Guid>("IntegrationTest:TestTenantId", Guid.Empty);

        var users = new Dictionary<TestRole, TestUserContext>();
        foreach (TestRole role in Enum.GetValues<TestRole>())
        {
            var section = cfg.GetSection($"IntegrationTest:Users:{role}");
            var id           = section.GetValue<int>("Id", -(int)role - 650);
            var fullName     = section.GetValue<string>("FullName", $"{role} Test User")!;
            var email        = section.GetValue<string>("Email",    $"{role.ToString().ToLower()}@domuwave.it")!;
            var isSystemUser = section.GetValue<bool>("IsSystemUser", false);
            users[role] = TestUserContext.Create(id, TenantId, fullName, email, role, isSystemUser);
        }
        Users = users;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTest.json"),
                optional: true, reloadOnChange: false);
            config.AddEnvironmentVariables("DOMUWAVE_TEST_");
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IntegrationTestFactory>(_ => this);
            services.AddSingleton<IStartupFilter, TestAuthStartupFilter>();

            services.PostConfigure<MvcOptions>(options =>
            {
                var tokenFilter = options.Filters
                    .OfType<TypeFilterAttribute>()
                    .FirstOrDefault(f => f.ImplementationType?.Name.Contains("TokenAuthorize") == true);
                if (tokenFilter != null)
                    options.Filters.Remove(tokenFilter);
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter());
            });

            // ── Mock IUserService — risponde per ogni utente di test per ID ──
            var cpqUsers = Users.Values.ToDictionary(
                u => (int)u.Id,
                u => new User
                {
                    Id              = (int)u.Id,
                    Name            = u.Username,
                    FirstName       = u.FirstName,
                    LastName        = u.LastName,
                    Email           = u.Email,
                    Token           = u.Token,
                    IsActive        = true,
                    IsAuthenticated = true,
                    IsSystemUser    = u.IsSystemUser,
                });

            services.RemoveAll<IUserService>();
            var userServiceMock = new Mock<IUserService>();
            userServiceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    cpqUsers.TryGetValue(id, out var u) ? u : cpqUsers[(int)Users[TestRole.DMW_SU].Id]);
            userServiceMock
                .Setup(s => s.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string token, CancellationToken _) =>
                    cpqUsers.Values.FirstOrDefault(u => u.Token == token)
                    ?? cpqUsers[(int)Users[TestRole.DMW_SU].Id]);
            services.AddScoped<IUserService>(_ => userServiceMock.Object);
        });
    }

    /// <summary>Creates an HttpClient authenticated as the given role.</summary>
    public HttpClient CreateClientAs(TestRole role)
    {
        var user   = Users[role];
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Tenant-Id",    TenantId.ToString());
        client.DefaultRequestHeaders.Add("Authorization",  $"Bearer {user.Token}");
        client.DefaultRequestHeaders.Add("X-Test-Role",    role.ToString());
        return client;
    }

    /// <summary>Default authenticated client (PRT_ADM).</summary>
    public HttpClient CreateAuthenticatedClient() => CreateClientAs(TestRole.DMW_SU);
}
