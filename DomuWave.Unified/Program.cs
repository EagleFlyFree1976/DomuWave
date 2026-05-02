using System.Reflection;
using Auth.Microservice;
using Auth.Services;
using QuestPDF.Infrastructure;
using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using CPQ.Core.Startups;
using DomuWave.Application.Filters;
using DomuWave.Services;
using DomuWave.Services.Settings;
using Hangfire;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;

IConfigurationRoot configuration;
var NETCoreEnv = OxCore.ChangeEnvironments(args);

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWindowsService();

var isService = args?.Contains("--service") ?? false;

var hostEnvironment = builder.Environment;
if (hostEnvironment.IsDevelopment())
{
    var path = $"appsettings.{hostEnvironment.EnvironmentName}_{Environment.MachineName}.json";

    configuration = builder
                    .Configuration.AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile(path, true, true)
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddEnvironmentVariables()
                    .Build();
}
else
{
    configuration = builder
                    .Configuration.AddJsonFile("appsettings.json", true, true)
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddEnvironmentVariables()
                    .Build();
}

_initSettings(out var _jobSettings, out var _oxCoreSettings);

var logger = new LoggerConfiguration()
             .ReadFrom.Configuration(configuration)
             .Enrich.WithThreadId()
             .Enrich
             .WithExceptionDetails(new DestructuringOptionsBuilder()
                                   .WithDefaultDestructurers()
                                   .WithDestructurers([new ApiExceptionDestructurer(destructureHttpContent: true)]))
             .CreateLogger();

logger.Information("Check env");
logger.Information("current ASPNETCORE_ENVIRONMENT: {env}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

if (!NETCoreEnv.IsNullOrEmpty())
    logger.Information("set ASPNETCORE_ENVIRONMENT: {env}", NETCoreEnv);
else
    logger.Information("ASPNETCORE_ENVIRONMENT: {env}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

logger.Information("Init build host");

try
{
    var services = builder.Services;

    services.AddSignalR(c => { });

    services.AddCors(options =>
        options.AddPolicy(
            "CorsPolicy",
            corsPolicyBuilder =>
            {
                _oxCoreSettings.SetDefault(corsPolicyBuilder);
            }
        )
    );

    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheManager, CacheManager>();

    // API controllers + Razor views (per la SPA shell)
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<TokenAuthorizeAttribute>();
    });

    // Razor Pages non usate ma AddControllersWithViews già copre le Views
    builder.Services.AddRazorPages();

    services.AddHeaderPropagation();

    if (hostEnvironment.IsProduction())
        builder.Services.AddHealthChecks();

    builder.Services.AddScoped<SystemBookHeaderFilter>();
    builder.Services.AddScoped<TenantHeaderFilter>();

    services.AddOxConfigureServices(
        configuration,
        _oxCoreSettings,
        hostEnvironment,
        false,
        _jobSettings
    );

    services.AddOxSimpleMediator(configuration);

    services.AddAuthServices(configuration);

    services.AddDomuWaveAppServices(_oxCoreSettings);

    services.AddScoped<DomuWave.Services.Clients.IAuthorizationClient, DomuWave.Microservice.Clients.LocalAuthorizationClient>();

    #region Swagger

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "DomuWave API" });
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
    });

    #endregion

    builder.Services.Configure<DomuWaveSettings>(builder.Configuration.GetSection("DomuWave"));

    builder.Host.UseSerilog(logger);

    var app = builder.Build();

    app.UseHeaderPropagation();

    app.UseCors("CorsPolicy");

    app.Use((context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/hangfire"))
        {
            if (_jobSettings?.JobServer is { IamJobServer: true } &&
                _jobSettings.JobServer.PathBase.NotIsNullOrEmpty())
            {
                context.Request.PathBase = new PathString(_jobSettings.JobServer.PathBase);
            }
        }
        return next();
    });

    app.UseOxCore(_oxCoreSettings);
    app.UseOxHangfireDashboard(_jobSettings);

    // Static files (Vue build output in wwwroot)
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "DomuWave API");
        c.RoutePrefix = "swagger";
    });

    // API controllers
    app.MapControllers();

    // Razor MVC per la shell HTML (serve index.cshtml che monta Vue)
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // SPA fallback: tutte le route non-API e non-file tornano all'index Vue
    app.MapFallbackToController("Index", "Home");

    app.Run();
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    logger?.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    logger?.Debug("Shutdown application");
}

void _initSettings(
    out JobSettings jobSettings,
    out OxCoreSettings oxCoreSettings
)
{
    oxCoreSettings = new OxCoreSettings();
    configuration.Bind("OxCore", oxCoreSettings);

    jobSettings = new JobSettings();
    configuration.Bind("JobSettings", jobSettings);
}

public partial class Program { }
