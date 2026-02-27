using System.Reflection;
using Auth.Microservice;
using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using CPQ.Core.Startups;
using DomuWave.Application.Filters;
using DomuWave.Services;
using DomuWave.Services.Consumers.Tenant;
using DomuWave.Services.Settings;
using Hangfire;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;

IConfigurationRoot configuration;
var NETCoreEnv = OxCore.ChangeEnvironments(args);

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

    // Unica registrazione di AddControllers con tutti i filtri globali
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<TokenAuthorizeAttribute>();
    });

    services.AddHeaderPropagation();

    if (hostEnvironment.IsProduction())
        builder.Services.AddHealthChecks();

    // SystemBookHeaderFilter usato solo via [ServiceFilter] su PrivateAdminControllerBase
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

    services.AddDomuWaveAppServices(_oxCoreSettings);

    #region Swagger

    services.AddSwaggerGen(c =>
    {
        string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DomuWave.Application.xml");
        c.IncludeXmlComments(_path);
        c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "DomuWave.Application API" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
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

    app.UseRouting();

    app.UseAuthorization();

    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "Auth.Microservice");
        c.RoutePrefix = "swagger";
    });

    app.MapControllers();

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
    logger?.Debug("Shutdown application ");
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