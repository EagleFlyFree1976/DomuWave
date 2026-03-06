using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsCategory;
using DomuWave.Services.Dto.ChartOfAccountsCategory;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateChartOfAccountsCategoryCommandConsumer
    : InMemoryConsumerBase<CreateChartOfAccountsCategoryCommand, ChartOfAccountsCategoryReadDto>
{
    private readonly IChartOfAccountsCategoryService _categoryService;
    private readonly ITenantService                  _tenantService;
    private readonly IUserService                    _userService;

    public CreateChartOfAccountsCategoryCommandConsumer(
        ISessionFactoryProvider          sessionFactoryProvider,
        IChartOfAccountsCategoryService  categoryService,
        ITenantService                   tenantService,
        IUserService                     userService) : base(sessionFactoryProvider)
    {
        _categoryService = categoryService;
        _tenantService   = tenantService;
        _userService     = userService;
    }

    protected override async Task<ChartOfAccountsCategoryReadDto> Consume(
        CreateChartOfAccountsCategoryCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome categoria è obbligatorio.");

        var tenant = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (tenant == null)
            throw new NotFoundException("Tenant non trovato.");

        var nameExists = await session.Query<ChartOfAccountsCategory>()
            .AnyAsync(x => x.Tenant.Id == command.TenantId
                        && x.Name == dto.Name.Trim()
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (nameExists)
            throw new ValidatorException($"Esiste già una categoria con nome '{dto.Name}'.");

        var entity  = dto.ToEntity(tenant);
        var created = await _categoryService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
