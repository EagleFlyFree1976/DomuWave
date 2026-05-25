using System.Configuration;
using System.Reflection;
using Auth.Microservice;
using Auth.Services;
using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using CPQ.Core.Startups;
using DomuWave.Application.Filters;
using DomuWave.Services;
using DomuWave.Services.Settings;
using Hangfire;
using DomuWave.Application.Middleware;
using DomuWave.Unified.Middleware;
using LicenseManager.Client.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Sinks.MSSqlServer;

IConfigurationRoot configuration;
var NETCoreEnv = OxCore.ChangeEnvironments(args);

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);


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

var logPath = Path.Combine(AppContext.BaseDirectory, "Logs", "domuwave-unified_.log");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.WithThreadId()
    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
        .WithDefaultDestructurers()
        .WithDestructurers([new ApiExceptionDestructurer(destructureHttpContent: true)]))
    .WriteTo.File(
        path: logPath,
        outputTemplate: "[{Timestamp:o}] |{Level:u4}| #{ThreadId} [{Application} - {MachineName}] {SourceContext} {Message}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 52428800,
        retainedFileCountLimit: 7,
        rollOnFileSizeLimit: true,
        shared: false).WriteTo.MSSqlServer(
        connectionString: configuration.GetConnectionString("Logs"),
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "AppLogs",
            AutoCreateSqlTable = true
        })
    .CreateLogger();

var logger = Log.Logger;

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
    builder.Services
        .AddLicenseManager(builder.Configuration.GetSection("LicenseManager"))
        .WithHttpHeaderTenantResolver();
    builder.Services.AddScoped<LicenseManager.Client.LicenseManagerHelper>();
    builder.Services.AddScoped<LicenseNotActivatedMiddleware>();
    builder.Services.AddScoped<LicenseManager.Client.TenantResolver.ISuperAdminChecker,
        DomuWave.Unified.Middleware.DomuWaveSuperAdminChecker>();
    builder.Host.UseSerilog(logger);

    var app = builder.Build();

    app.UseHeaderPropagation();

    // Static files — usa sempre il percorso fisico esplicito basato sull'exe
    var wwwrootPath = app.Environment.IsDevelopment()
        ? Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "DomuWave.Web", "wwwroot"))
        : Path.Combine(AppContext.BaseDirectory, "wwwroot");

    if (Directory.Exists(wwwrootPath))
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(wwwrootPath),
            RequestPath  = ""
        });

    logger.Information("WebRoot: {wwwroot} (exists: {exists})", wwwrootPath, Directory.Exists(wwwrootPath));

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
    app.UseLicenseManager();
    app.UseMiddleware<LicenseNotActivatedMiddleware>();
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
    app.MapLicenseManagerWebhook();

    string GetWwwroot() => app.Environment.IsDevelopment()
        ? Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "DomuWave.Web", "wwwroot"))
        : Path.Combine(AppContext.BaseDirectory, "wwwroot");

    async Task ServeSpaIndex(HttpContext ctx)
    {
        ctx.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        ctx.Response.Headers["Pragma"] = "no-cache";
        ctx.Response.ContentType = "text/html";
        await ctx.Response.SendFileAsync(Path.Combine(GetWwwroot(), "index.html"));
    }

    // SPA: serve index.html per tutte le route non-API (inclusa /)
    app.MapGet("/", ServeSpaIndex);
    app.MapFallback(async context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 404;
            return;
        }
        await ServeSpaIndex(context);
    });
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

//public partial class Program { }
