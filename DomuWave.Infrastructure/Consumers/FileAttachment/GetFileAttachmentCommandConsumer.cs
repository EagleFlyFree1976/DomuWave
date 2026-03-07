using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FileAttachment;
using DomuWave.Services.Dto.FileAttachment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.FileAttachment;

public class GetFileAttachmentCommandConsumer
    : InMemoryConsumerBase<GetFileAttachmentCommand, FileAttachmentReadDto>
{
    private readonly IUserService _userService;

    public GetFileAttachmentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<FileAttachmentReadDto> Consume(
        GetFileAttachmentCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var attachment = await session.Query<Models.FileAttachment>()
            .Where(f => f.Id == command.AttachmentId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (attachment == null)
            throw new NotFoundException("Allegato non trovato");

        // Carica esplicitamente il contenuto
        var content = attachment.Content;

        return attachment.ToReadDto(includeContent: true);
    }
}
