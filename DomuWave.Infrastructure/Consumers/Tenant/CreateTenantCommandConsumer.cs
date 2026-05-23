using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using LicenseManager.Client.Services;
using Microsoft.Extensions.Logging;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Tenant;

public class CreateTenantCommandConsumer
    : InMemoryConsumerBase<CreateTenantCommand, TenantReadDto>
{
    private readonly ITenantService                          _tenantService;
    private readonly IUserService                            _userService;
    private readonly IChartOfAccountsCategoryService         _categoryService;
    private readonly IChartOfAccountsCategoryTemplateService _templateService;
    private readonly IChartOfAccountsTemplateSeedService     _coaTemplateSeed;
    private readonly ILicenseManagerClient                   _licenseClient;
    private readonly ILogger<CreateTenantCommandConsumer>    _logger;

    public CreateTenantCommandConsumer(
        ISessionFactoryProvider                  sessionFactoryProvider,
        ITenantService                           tenantService,
        IUserService                             userService,
        IChartOfAccountsCategoryService          categoryService,
        IChartOfAccountsCategoryTemplateService  templateService,
        IChartOfAccountsTemplateSeedService      coaTemplateSeed,
        ILicenseManagerClient                    licenseClient,
        ILogger<CreateTenantCommandConsumer>     logger) : base(sessionFactoryProvider)
    {
        _tenantService   = tenantService;
        _userService     = userService;
        _categoryService = categoryService;
        _templateService = templateService;
        _coaTemplateSeed = coaTemplateSeed;
        _licenseClient   = licenseClient;
        _logger          = logger;
    }

    protected override async Task<TenantReadDto> Consume(
        CreateTenantCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Validazione business: codice univoco
        var existing = await _tenantService
            .GetByCodeAsync(command.Code, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (existing != null)
            throw new ValidatorException($"Esiste già un tenant con il codice '{command.Code}'.");

        var entity = new Models.Tenant
        {
            Name     = command.Name,
            Code     = command.Code,
            IsActive = command.IsActive,
        };

        entity.Trace(currentUser);

        var created = await _tenantService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Seed categorie dal template
        var templates = await _templateService
            .GetActiveAsync(currentUser, cancellationToken)
            .ConfigureAwait(false);

        foreach (var template in templates)
        {
            var category = template.ToCategory(created);
            await _categoryService
                .CreateAsync(category, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        // Seed template piano dei conti standard
        await _coaTemplateSeed.SeedAsync(created, cancellationToken).ConfigureAwait(false);

        // Notifica LicenseManager della creazione del tenant (fire-and-forget: non blocca se LM è down)
        try
        {
            var ownerEmail = currentUser.Email ?? string.Empty;
            var result = await _licenseClient
                .NotifyTenantCreatedAsync(created.Id, ownerEmail, created.Name, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Success)
                _logger.LogWarning(
                    "LicenseManager: notifica tenant {TenantId} fallita: {Error}",
                    created.Id, result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "LicenseManager non raggiungibile durante la creazione del tenant {TenantId}. Il tenant verrà registrato manualmente.",
                created.Id);
        }

        return created.ToReadDto();
    }
}
