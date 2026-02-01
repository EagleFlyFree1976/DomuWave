 

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
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<ITransactionStatusService, TransactionStatusService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IExchangeRateHistoryService, ExchangeRateHistoryService>();
        services.AddScoped<IAccountTypeService, AccountTypeService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IExchangeRateClient, ExchangeRateApiClient>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<ServiceJob, ServiceJob>();

 

        return services;
    }
}