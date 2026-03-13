using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsTemplate;
using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateChartOfAccountsTemplateCommandConsumer
    : InMemoryConsumerBase<UpdateChartOfAccountsTemplateCommand, ChartOfAccountsTemplateReadDto>
{
    private readonly IUserService _userService;

    public UpdateChartOfAccountsTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<ChartOfAccountsTemplateReadDto> Consume(
        UpdateChartOfAccountsTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<ChartOfAccountsTemplate>()
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Template non trovato.");

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome del template è obbligatorio.");

        var nameExists = await session.Query<ChartOfAccountsTemplate>()
            .AnyAsync(t => t.Id != command.Id
                        && t.Tenant.Id == entity.Tenant.Id
                        && t.Name == dto.Name.Trim()
                        && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (nameExists)
            throw new ValidatorException($"Esiste già un template con nome '{dto.Name}'.");

        // Se IsDefault=true, togli il flag agli altri template dello stesso tenant
        if (dto.IsDefault)
        {
            var currentDefaults = await session.Query<ChartOfAccountsTemplate>()
                .Where(t => t.Tenant.Id == entity.Tenant.Id && t.Id != command.Id && t.IsDefault && !t.IsDeleted)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var other in currentDefaults)
            {
                other.IsDefault = false;
                other.Trace(currentUser);
                await session.UpdateAsync(other, cancellationToken).ConfigureAwait(false);
            }
        }

        entity.ApplyUpdate(dto);
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        // Eager-load items for ItemCount
        await NHibernate.NHibernateUtil.InitializeAsync(entity.Items, cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}
