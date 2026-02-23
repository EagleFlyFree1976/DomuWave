 

using System.Net.Http.Headers;
using DomuWave.Services.Clients;
using DomuWave.Services.Implementations;
using DomuWave.Services.Interfaces;
using CPQ.Core;
using CPQ.Core.Handlers;
using CPQ.Core.Services;
using CPQ.Core.Services.Clients;
using CPQ.Core.Settings;
using CPQ.Core.Startups;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace DomuWave.Services;

public static class Startup
{
    private static void _initClient(IServiceCollection services, OxCoreSettings oxCoreSettings)
    {
        //  services.AddScoped<TokenHttpMessageHandler>();

        services.AddRefitClient<IAuthorizationClient>(new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer()
        }).ConfigureHttpClient(client =>
        {
            client.BaseAddress =
                            new Uri(oxCoreSettings.Microservices[MicroserviceKeys.authorization_base_uri]
                                   );
            client.DefaultRequestHeaders.Accept
                  .Clear();
            client.DefaultRequestHeaders.Accept
                  .Add(new
                                       MediaTypeWithQualityHeaderValue("application/json"));
        }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddHeaderPropagation()
                .AddHttpMessageHandler<HttpLoggingHandler>()
                .AddPolicyHandler(OxCoreClientSettings.GetRetryPolicy());


        
    }

    public static IServiceCollection AddDomuWaveAppServices(this IServiceCollection services, OxCoreSettings _coreSettings)
    {
         
        _initClient(services,_coreSettings);
        
        services.AddScoped<IMenuService, MenuService>();
       


        // Tenant Management Services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IUnitTenantService, UnitTenantService>();

        // Document Management Services
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentAccessService, DocumentAccessService>();

        // Communication Services
        services.AddScoped<ICommunicationService, CommunicationService>();
        services.AddScoped<ICommunicationReadService, CommunicationReadService>();

        // Financial Services
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IExpenseService, ExpenseService>();

        // Condominium Management Services
        services.AddScoped<ICondominiumService, CondominiumService>();
        services.AddScoped<ICondominiumCadastralDataService, CondominiumCadastralDataService>();
        services.AddScoped<IUnitMillesimalService, UnitMillesimalService>();

        // Supplier Services
        services.AddScoped<ISupplierService, SupplierService>();

        // External Clients
        services.AddScoped<IExchangeRateClient, ExchangeRateApiClient>();
        services.AddScoped<IUserTenantService, UserTenantService>();


        return services;
    }
}