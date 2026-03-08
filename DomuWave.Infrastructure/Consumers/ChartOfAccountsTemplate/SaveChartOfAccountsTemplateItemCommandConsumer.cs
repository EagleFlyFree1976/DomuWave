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

public class SaveChartOfAccountsTemplateItemCommandConsumer
    : InMemoryConsumerBase<SaveChartOfAccountsTemplateItemCommand, ChartOfAccountsTemplateItemReadDto>
{
    private readonly IUserService _userService;

    public SaveChartOfAccountsTemplateItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<ChartOfAccountsTemplateItemReadDto> Consume(
        SaveChartOfAccountsTemplateItemCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var dto         = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ValidatorException("Il codice è obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome è obbligatorio.");

        var template = await session.Query<ChartOfAccountsTemplate>()
            .FirstOrDefaultAsync(t => t.Id == dto.TemplateId && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (template == null)
            throw new NotFoundException("Template non trovato.");

        ChartOfAccountsTemplateItem? parent = null;
        if (dto.ParentItemId.HasValue)
        {
            parent = await session.Query<ChartOfAccountsTemplateItem>()
                .FirstOrDefaultAsync(i => i.Id == dto.ParentItemId.Value && !i.IsDeleted, cancellationToken).ConfigureAwait(false);
            if (parent == null)
                throw new NotFoundException("Voce padre non trovata.");
        }

        // Check code uniqueness within template
        var codeQuery = session.Query<ChartOfAccountsTemplateItem>()
            .Where(i => i.Template.Id == dto.TemplateId && i.Code == dto.Code.Trim() && !i.IsDeleted);
        if (command.Id.HasValue)
            codeQuery = codeQuery.Where(i => i.Id != command.Id.Value);
        var codeExists = await codeQuery.AnyAsync(cancellationToken).ConfigureAwait(false);
        if (codeExists)
            throw new ValidatorException($"Esiste già una voce con codice '{dto.Code}' in questo template.");

        ChartOfAccountsTemplateItem entity;

        if (command.Id.HasValue)
        {
            entity = await session.Query<ChartOfAccountsTemplateItem>()
                .FirstOrDefaultAsync(i => i.Id == command.Id.Value && !i.IsDeleted, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Voce non trovata.");

            entity.ApplyUpdate(dto, parent);
            entity.Trace(currentUser);
            await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            entity = dto.ToEntity(template, parent, template.Tenant);
            entity.Trace(currentUser);
            await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        return entity.ToReadDto();
    }
}
