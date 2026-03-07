using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FileAttachment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.FileAttachment;

public class DeleteFileAttachmentCommandConsumer
    : InMemoryConsumerBase<DeleteFileAttachmentCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteFileAttachmentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        DeleteFileAttachmentCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var attachment = await session.Query<Models.FileAttachment>()
            .Where(f => f.Id == command.AttachmentId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (attachment == null)
            throw new NotFoundException("Allegato non trovato");

        if (attachment.Content != null)
            await session.DeleteAsync(attachment.Content, cancellationToken).ConfigureAwait(false);

        await session.DeleteAsync(attachment, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
