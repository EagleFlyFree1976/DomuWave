using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsTemplate;
using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateChartOfAccountsTemplateCommandConsumer
    : InMemoryConsumerBase<CreateChartOfAccountsTemplateCommand, ChartOfAccountsTemplateReadDto>
{
    private readonly IUserService   _userService;
    private readonly ITenantService _tenantService;

    public CreateChartOfAccountsTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        ITenantService          tenantService) : base(sessionFactoryProvider)
    {
        _userService   = userService;
        _tenantService = tenantService;
    }

    protected override async Task<ChartOfAccountsTemplateReadDto> Consume(
        CreateChartOfAccountsTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome del template è obbligatorio.");

        var tenant = await _tenantService.GetByIdAsync(command.TenantId, currentUser , cancellationToken).ConfigureAwait(false);
        if (tenant == null)
            throw new NotFoundException("Tenant non trovato.");

        var nameExists = await session.Query<ChartOfAccountsTemplate>()
            .AnyAsync(t => t.Tenant.Id == command.TenantId
                        && t.Name == dto.Name.Trim()
                        && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (nameExists)
            throw new ValidatorException($"Esiste già un template con nome '{dto.Name}'.");

        // Se IsDefault=true, togli il flag agli altri template dello stesso tenant
        if (dto.IsDefault)
        {
            var currentDefaults = await session.Query<ChartOfAccountsTemplate>()
                .Where(t => t.Tenant.Id == command.TenantId && t.IsDefault && !t.IsDeleted)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var other in currentDefaults)
            {
                other.IsDefault = false;
                other.Trace(currentUser);
                await session.UpdateAsync(other, cancellationToken).ConfigureAwait(false);
            }
        }

        var entity = dto.ToEntity(tenant);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}
