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
/// <see cref="WebApplicationFactory{TEntryPoint}"/> che avvia il pipeline completo di
/// <c>DomuWave.Application</c> in un server in-process per i test di integrazione.
///
/// COSA FA:
/// <list type="bullet">
///   <item>Sostituisce l'autenticazione reale con il <see cref="TestAuthStartupFilter"/>
///         che inietta l'utente di test in base all'header <c>X-Test-Role</c>.</item>
///   <item>Rimuove il filtro <c>TokenAuthorizeFilter</c> (verifica JWT reale) e lo sostituisce
///         con <c>AllowAnonymousFilter</c>: l'autenticazione è gestita interamente dal middleware
///         di test.</item>
///   <item>Sostituisce <c>IUserService</c> con un mock Moq che risponde per ogni utente di test
///         configurato, sia per lookup per Id che per Token.</item>
///   <item>Legge la configurazione da <c>appsettings.IntegrationTest.json</c> e dalle variabili
///         d'ambiente con prefisso <c>DOMUWAVE_TEST_</c>.</item>
/// </list>
///
/// UTENTI DI TEST: la factory crea un utente per ogni valore di <see cref="TestRole"/>.
/// I dati (id, nome, email) sono letti dalla configurazione; se assenti, usa valori di
/// fallback ragionevoli (id negativo = -(int)role - 650, email = role@domuwave.it).
///
/// CONDIVISIONE: la factory è registrata come <c>[CollectionFixture]</c> e condivisa tra
/// tutti i test della collection "Integration", quindi l'applicazione viene avviata
/// una sola volta per tutta la suite (startup significativo con NHibernate).
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Dizionario di tutti gli utenti di test, uno per ruolo.
    /// Popolato nel costruttore dalla configurazione.
    /// </summary>
    public IReadOnlyDictionary<TestRole, TestUserContext> Users { get; }

    /// <summary>
    /// Utente di test predefinito usato dal <see cref="IntegrationTestBase.Client"/> standard.
    /// Corrisponde al ruolo <see cref="TestRole.DMW_SU"/> (superuser di sistema con accesso totale).
    /// </summary>
    public TestUserContext TestUser => Users[TestRole.DMW_SU];

    /// <summary>
    /// TenantId del tenant di test, letto da <c>IntegrationTest:TestTenantId</c> nella configurazione.
    /// Viene iniettato in tutti i client HTTP come header <c>X-Tenant-Id</c>.
    /// </summary>
    public Guid TenantId { get; }

    public IntegrationTestFactory()
    {
        // Legge la configurazione di test: prima da appsettings.IntegrationTest.json,
        // poi da variabili d'ambiente (override per CI/CD).
        var cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.IntegrationTest.json", optional: true)
            .AddEnvironmentVariables("DOMUWAVE_TEST_")
            .Build();

        TenantId = cfg.GetValue<Guid>("IntegrationTest:TestTenantId", Guid.Empty);

        // Crea un TestUserContext per ogni ruolo definito nell'enum TestRole.
        // Se la sezione di configurazione non esiste, usa valori di fallback.
        var users = new Dictionary<TestRole, TestUserContext>();
        foreach (TestRole role in Enum.GetValues<TestRole>())
        {
            var section      = cfg.GetSection($"IntegrationTest:Users:{role}");
            var id           = section.GetValue<int>("Id", -(int)role - 650);       // id negativo = no conflitti con dati reali
            var fullName     = section.GetValue<string>("FullName", $"{role} Test User")!;
            var email        = section.GetValue<string>("Email",    $"{role.ToString().ToLower()}@domuwave.it")!;
            var isSystemUser = section.GetValue<bool>("IsSystemUser", false);
            users[role] = TestUserContext.Create(id, TenantId, fullName, email, role, isSystemUser);
        }
        Users = users;
    }

    /// <summary>
    /// Configura il Web Host per l'ambiente di test:
    /// <list type="number">
    ///   <item>Imposta environment = "IntegrationTest" (carica appsettings.IntegrationTest.json).</item>
    ///   <item>Registra il <see cref="TestAuthStartupFilter"/> che inietta l'utente corrente
    ///         in <c>HttpContext.Items</c> in base all'header <c>X-Test-Role</c>.</item>
    ///   <item>Rimuove <c>TokenAuthorizeFilter</c> (verifica JWT) e aggiunge <c>AllowAnonymousFilter</c>.</item>
    ///   <item>Sostituisce <c>IUserService</c> con un mock che risponde per tutti gli utenti di test.</item>
    /// </list>
    /// </summary>
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
            // Rende la factory accessibile al middleware di test tramite DI
            services.AddSingleton<IntegrationTestFactory>(_ => this);

            // Inietta il middleware di autenticazione finta (prima del pipeline MVC)
            services.AddSingleton<IStartupFilter, TestAuthStartupFilter>();

            // Rimuove il filtro di autorizzazione JWT e abilita l'accesso anonimo:
            // la "autenticazione" è gestita dal TestAuthStartupFilter che imposta
            // CurrentUser in HttpContext.Items in base all'header X-Test-Role.
            services.PostConfigure<MvcOptions>(options =>
            {
                var tokenFilter = options.Filters
                    .OfType<TypeFilterAttribute>()
                    .FirstOrDefault(f => f.ImplementationType?.Name.Contains("TokenAuthorize") == true);
                if (tokenFilter != null)
                    options.Filters.Remove(tokenFilter);
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter());
            });

            // ── Mock IUserService ──────────────────────────────────────────────────
            // Sostituisce l'implementazione reale (che richiederebbe DB + Redis) con un mock
            // che risponde direttamente dai dati di test in memoria.
            // GetByIdAsync: cerca per Id; se non trovato, torna al DMW_SU (default).
            // GetByTokenAsync: cerca per Token; se non trovato, torna al DMW_SU.
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

    /// <summary>
    /// Crea un <see cref="HttpClient"/> autenticato come il ruolo specificato.
    /// Aggiunge gli header:
    /// <list type="bullet">
    ///   <item><c>X-Tenant-Id</c>: il <see cref="TenantId"/> del tenant di test.</item>
    ///   <item><c>Authorization: Bearer {token}</c>: il token dell'utente di test.</item>
    ///   <item><c>X-Test-Role</c>: il ruolo, letto dal <see cref="TestAuthStartupFilter"/>
    ///         per impostare l'utente corrente nel pipeline.</item>
    /// </list>
    /// </summary>
    public HttpClient CreateClientAs(TestRole role)
    {
        var user   = Users[role];
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Tenant-Id",   TenantId.ToString());
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {user.Token}");
        client.DefaultRequestHeaders.Add("X-Test-Role",   role.ToString());
        return client;
    }

    /// <summary>
    /// Crea il client HTTP predefinito per i test, autenticato come <see cref="TestRole.DMW_SU"/>
    /// (superuser con accesso completo a tutti gli endpoint).
    /// </summary>
    public HttpClient CreateAuthenticatedClient() => CreateClientAs(TestRole.DMW_SU);
}
