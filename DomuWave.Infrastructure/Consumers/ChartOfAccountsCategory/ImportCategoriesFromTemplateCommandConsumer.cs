using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsCategory;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class ImportCategoriesFromTemplateCommandConsumer
    : InMemoryConsumerBase<ImportCategoriesFromTemplateCommand, int>
{
    private readonly IChartOfAccountsCategoryService _categoryService;
    private readonly ITenantService                  _tenantService;
    private readonly IUserService                    _userService;

    public ImportCategoriesFromTemplateCommandConsumer(
        ISessionFactoryProvider          sessionFactoryProvider,
        IChartOfAccountsCategoryService  categoryService,
        ITenantService                   tenantService,
        IUserService                     userService) : base(sessionFactoryProvider)
    {
        _categoryService = categoryService;
        _tenantService   = tenantService;
        _userService     = userService;
    }

    protected override async Task<int> Consume(
        ImportCategoriesFromTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (tenant == null)
            throw new NotFoundException("Tenant non trovato.");

        // Nomi già presenti per questo tenant (case-insensitive)
        var existingNames = await session.Query<ChartOfAccountsCategory>()
            .Where(x => x.Tenant.Id == command.TenantId && !x.IsDeleted)
            .Select(x => x.Name.ToLower())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var templates = await session.Query<ChartOfAccountsCategoryTemplate>()
            .Where(x => command.TemplateIds.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        int imported = 0;
        foreach (var template in templates)
        {
            if (existingNames.Contains(template.Name.ToLower()))
                continue;

            var category = template.ToCategory(tenant);
            await _categoryService
                .CreateAsync(category, currentUser, cancellationToken)
                .ConfigureAwait(false);

            existingNames.Add(template.Name.ToLower());
            imported++;
        }

        return imported;
    }
}
