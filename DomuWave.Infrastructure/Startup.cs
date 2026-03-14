using System.Net.Http.Headers;
using DomuWave.Services.Clients;
using DomuWave.Services.Consumers.Tenant;
using DomuWave.Services.Implementations;
using DomuWave.Services.Implementations.FileStorage;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.FileStorage;
using DomuWave.Services.Settings;
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
        services.AddRefitClient<IAuthorizationClient>(new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer()
        }).ConfigureHttpClient(client =>
        {
            client.BaseAddress =
                new Uri(oxCoreSettings.Microservices[MicroserviceKeys.authorization_base_uri]);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
          .AddHeaderPropagation()
          .AddHttpMessageHandler<HttpLoggingHandler>()
          .AddPolicyHandler(OxCoreClientSettings.GetRetryPolicy());
    }

    public static IServiceCollection AddDomuWaveAppServices(
        this IServiceCollection services,
        OxCoreSettings coreSettings,
        Action<DynamicFileSettings>? configureDynamicFile = null)
    {
        _initClient(services, coreSettings);

        // ─── Dynamic File Repository ───────────────────────────────────────────
        services.Configure<DynamicFileSettings>(opts => configureDynamicFile?.Invoke(opts));
        services.AddScoped<IFileStorageProvider, DatabaseFileStorageProvider>();
        services.AddScoped<IFileStorageProvider, FileSystemStorageProvider>();
        services.AddScoped<IFileStorageProviderFactory, FileStorageProviderFactory>();
        services.AddScoped<IDynamicFileService, DynamicFileService>();

        // ─── Services (repository layer) ──────────────────────────────────────

    
 

        // External clients
        services.AddScoped<IExchangeRateClient, ExchangeRateApiClient>();
        


        
        services.AddScoped<IBudgetItemService, BudgetItemService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IChartOfAccountsCategoryTemplateService, ChartOfAccountsCategoryTemplateService>();
        services.AddScoped<IChartOfAccountsCategoryService, ChartOfAccountsCategoryService>();
        services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
        services.AddScoped<IChartOfAccountsTemplateSeedService, ChartOfAccountsTemplateSeedService>();
        services.AddScoped<ICommunicationReadService, CommunicationReadService>();
        services.AddScoped<ICommunicationService, CommunicationService>();
        services.AddScoped<ICondominiumAddressService, CondominiumAddressService>();
        services.AddScoped<ICondominiumCadastralDataService, CondominiumCadastralDataService>();
        services.AddScoped<ICondominiumFeeService, CondominiumFeeService>();
        services.AddScoped<ICondominiumInstallmentService, CondominiumInstallmentService>();
        services.AddScoped<ICondominiumService, CondominiumService>();
        services.AddScoped<IDocumentAccessService, DocumentAccessService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IExtraordinaryWorkService, ExtraordinaryWorkService>();
        services.AddScoped<IExpenseAllocationService, ExpenseAllocationService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IFiscalYearService, FiscalYearService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMillesimalTableService, MillesimalTableService>();
        services.AddScoped<IRealEstateUnitService, RealEstateUnitService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<ISupplierContractService, SupplierContractService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IUnitMillesimalService, UnitMillesimalService>();
        services.AddScoped<IUnitOwnerService, UnitOwnerService>();
        services.AddScoped<IWorkQuoteDocumentService, WorkQuoteDocumentService>();
        services.AddScoped<IWorkQuoteService, WorkQuoteService>();
        services.AddScoped<IUnitTenantService, UnitTenantService>();
        services.AddScoped<IUserTenantService, UserTenantService>();
        return services;
    }
}
