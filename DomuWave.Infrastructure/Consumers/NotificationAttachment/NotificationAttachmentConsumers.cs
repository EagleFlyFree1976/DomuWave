using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.NotificationAttachment;
using DomuWave.Services.Dto.NotificationAttachment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.NotificationAttachment;

// ── GET ATTACHMENTS BY NOTIFICATION ──────────────────────────────────────────

public class GetNotificationAttachmentsCommandConsumer
    : InMemoryConsumerBase<GetNotificationAttachmentsCommand, IList<NotificationAttachmentReadDto>>
{
    private readonly IUserService _userService;

    public GetNotificationAttachmentsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<NotificationAttachmentReadDto>> Consume(
        GetNotificationAttachmentsCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<DomuWave.Services.Models.NotificationAttachment>()
            .Where(a => a.Notification.Id == command.NotificationId && !a.IsDeleted)
            .OrderBy(a => a.CreationDate)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return list.Select(a => a.ToReadDto()).ToList();
    }
}

// ── ADD ATTACHMENT ────────────────────────────────────────────────────────────

public class AddNotificationAttachmentCommandConsumer
    : InMemoryConsumerBase<AddNotificationAttachmentCommand, NotificationAttachmentReadDto>
{
    private readonly IUserService _userService;

    public AddNotificationAttachmentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<NotificationAttachmentReadDto> Consume(
        AddNotificationAttachmentCommand command,
        IMediationContext                 mediationContext,
        CancellationToken                cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var notification = await session.Query<DomuWave.Services.Models.CommunicationNotification>()
            .FirstOrDefaultAsync(n => n.Id == command.NotificationId && !n.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Notifica non trovata.");

        if (notification.Status >= 1)
            throw new ValidatorException("Non è possibile modificare una notifica già inviata.");

        var document = await session.Query<DomuWave.Services.Models.Document>()
            .FirstOrDefaultAsync(d => d.Id == command.DocumentId && !d.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Documento non trovato.");

        var duplicate = await session.Query<DomuWave.Services.Models.NotificationAttachment>()
            .AnyAsync(a => a.Notification.Id == command.NotificationId && a.Document.Id == command.DocumentId && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (duplicate)
            throw new ValidatorException("Il documento è già allegato a questa notifica.");
        
        var entity = new DomuWave.Services.Models.NotificationAttachment
        {
            Tenant       = document.Tenant,
            Notification = notification,
            Document     = document,
        };
        entity.Trace(currentUser);
        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}

// ── REMOVE ATTACHMENT ─────────────────────────────────────────────────────────

public class RemoveNotificationAttachmentCommandConsumer
    : InMemoryConsumerBase<RemoveNotificationAttachmentCommand, bool>
{
    private readonly IUserService _userService;

    public RemoveNotificationAttachmentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        RemoveNotificationAttachmentCommand command,
        IMediationContext                    mediationContext,
        CancellationToken                   cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<DomuWave.Services.Models.NotificationAttachment>()
            .FirstOrDefaultAsync(a => a.Id == command.Id && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return false;

        if (entity.Notification?.Status >= 1)
            throw new ValidatorException("Non è possibile modificare una notifica già inviata.");

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
