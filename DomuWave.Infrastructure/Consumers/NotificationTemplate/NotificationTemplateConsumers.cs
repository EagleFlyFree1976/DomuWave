using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.NotificationTemplate;
using DomuWave.Services.Dto.NotificationTemplate;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── GET BY CONDOMINIUM ────────────────────────────────────────────────────────

public class GetNotificationTemplatesByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetNotificationTemplatesByCondominiumCommand, IList<NotificationTemplateReadDto>>
{
    private readonly IUserService _userService;

    public GetNotificationTemplatesByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<NotificationTemplateReadDto>> Consume(
        GetNotificationTemplatesByCondominiumCommand command,
        IMediationContext                             mediationContext,
        CancellationToken                            cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<NotificationTemplate>()
            .Where(t => t.Condominium.Id == command.CondominiumId && !t.IsDeleted)
            .OrderBy(t => t.CommunicationType)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return list.Select(t => t.ToReadDto()).ToList();
    }
}

// ── GET BY ID ─────────────────────────────────────────────────────────────────

public class GetNotificationTemplateByIdCommandConsumer
    : InMemoryConsumerBase<GetNotificationTemplateByIdCommand, NotificationTemplateReadDto?>
{
    private readonly IUserService _userService;

    public GetNotificationTemplateByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<NotificationTemplateReadDto?> Consume(
        GetNotificationTemplateByIdCommand command,
        IMediationContext                   mediationContext,
        CancellationToken                  cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<NotificationTemplate>()
            .Where(t => t.Id == command.TemplateId && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return entity?.ToReadDto();
    }
}

// ── CREATE ────────────────────────────────────────────────────────────────────

public class CreateNotificationTemplateCommandConsumer
    : InMemoryConsumerBase<CreateNotificationTemplateCommand, NotificationTemplateReadDto>
{
    private readonly IUserService _userService;

    public CreateNotificationTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<NotificationTemplateReadDto> Consume(
        CreateNotificationTemplateCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condominium = await session.GetAsync<Models.Condominium>(command.Dto.CondominiumId, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome del template è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.SubjectTemplate))
            throw new ValidatorException("L'oggetto del template è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.BodyTemplate))
            throw new ValidatorException("Il corpo del template è obbligatorio.");

        // Se IsDefault, rimuovi il flag dagli altri template dello stesso tipo
        if (command.Dto.IsDefault)
            await ClearDefaultForType(command.Dto.CondominiumId, command.Dto.CommunicationType, null, cancellationToken);

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant);
        entity.Trace(currentUser);

        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }

    private async Task ClearDefaultForType(int condominiumId, string type, int? exceptId, CancellationToken ct)
    {
        var existing = await session.Query<NotificationTemplate>()
            .Where(t => t.Condominium.Id == condominiumId && t.CommunicationType == type
                     && t.IsDefault && !t.IsDeleted && (exceptId == null || t.Id != exceptId))
            .ToListAsync(ct).ConfigureAwait(false);

        foreach (var t in existing)
        {
            t.IsDefault = false;
            await session.UpdateAsync(t, ct).ConfigureAwait(false);
        }
    }
}

// ── UPDATE ────────────────────────────────────────────────────────────────────

public class UpdateNotificationTemplateCommandConsumer
    : InMemoryConsumerBase<UpdateNotificationTemplateCommand, NotificationTemplateReadDto?>
{
    private readonly IUserService _userService;

    public UpdateNotificationTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<NotificationTemplateReadDto?> Consume(
        UpdateNotificationTemplateCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<NotificationTemplate>()
            .Where(t => t.Id == command.Id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null) return null;

        if (command.Dto.IsDefault == true)
        {
            var others = await session.Query<NotificationTemplate>()
                .Where(t => t.Condominium.Id == entity.Condominium.Id
                         && t.CommunicationType == entity.CommunicationType
                         && t.IsDefault && !t.IsDeleted && t.Id != entity.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var t in others) { t.IsDefault = false; await session.UpdateAsync(t, cancellationToken).ConfigureAwait(false); }
        }

        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}

// ── DELETE ────────────────────────────────────────────────────────────────────

public class DeleteNotificationTemplateCommandConsumer
    : InMemoryConsumerBase<DeleteNotificationTemplateCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteNotificationTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        DeleteNotificationTemplateCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<NotificationTemplate>()
            .Where(t => t.Id == command.TemplateId && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
