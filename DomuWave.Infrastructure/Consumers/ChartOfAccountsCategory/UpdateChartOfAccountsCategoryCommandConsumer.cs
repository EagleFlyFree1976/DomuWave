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

public class UpdateChartOfAccountsCategoryCommandConsumer
    : InMemoryConsumerBase<UpdateChartOfAccountsCategoryCommand, ChartOfAccountsCategoryReadDto>
{
    private readonly IChartOfAccountsCategoryService _categoryService;
    private readonly IUserService                    _userService;

    public UpdateChartOfAccountsCategoryCommandConsumer(
        ISessionFactoryProvider          sessionFactoryProvider,
        IChartOfAccountsCategoryService  categoryService,
        IUserService                     userService) : base(sessionFactoryProvider)
    {
        _categoryService = categoryService;
        _userService     = userService;
    }

    protected override async Task<ChartOfAccountsCategoryReadDto> Consume(
        UpdateChartOfAccountsCategoryCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _categoryService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Categoria non trovata.");

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome categoria è obbligatorio.");

        var nameExists = await session.Query<ChartOfAccountsCategory>()
            .AnyAsync(x => x.Tenant.Id == entity.Tenant.Id
                        && x.Name      == dto.Name.Trim()
                        && x.Id        != command.Id
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (nameExists)
            throw new ValidatorException($"Esiste già una categoria con nome '{dto.Name}'.");

        entity.ApplyUpdate(dto);
        var updated = await _categoryService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
