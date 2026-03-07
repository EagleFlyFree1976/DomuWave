using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class DeleteQuoteDocumentCommandConsumer
    : InMemoryConsumerBase<DeleteQuoteDocumentCommand, bool>
{
    private readonly IWorkQuoteDocumentService _documentService;
    private readonly IUserService              _userService;

    public DeleteQuoteDocumentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IWorkQuoteDocumentService documentService,
        IUserService userService) : base(sessionFactoryProvider)
    { _documentService = documentService; _userService = userService; }

    protected override async Task<bool> Consume(
        DeleteQuoteDocumentCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return await _documentService.DeleteAsync(command.DocumentId, currentUser, cancellationToken).ConfigureAwait(false);
    }
}
