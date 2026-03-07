using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Dto.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class AddQuoteDocumentCommandConsumer
    : InMemoryConsumerBase<AddQuoteDocumentCommand, WorkQuoteDocumentReadDto>
{
    private readonly IWorkQuoteDocumentService _documentService;
    private readonly IWorkQuoteService         _quoteService;
    private readonly IUserService              _userService;

    public AddQuoteDocumentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IWorkQuoteDocumentService documentService,
        IWorkQuoteService quoteService,
        IUserService userService) : base(sessionFactoryProvider)
    { _documentService = documentService; _quoteService = quoteService; _userService = userService; }

    protected override async Task<WorkQuoteDocumentReadDto> Consume(
        AddQuoteDocumentCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var quote = await _quoteService.GetByIdAsync(command.Dto.QuoteId, currentUser, cancellationToken).ConfigureAwait(false);
        if (quote == null)
            throw new NotFoundException("Preventivo non trovato");

        var entity  = command.Dto.ToEntity(quote);
        var created = await _documentService.CreateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);
        return created.ToReadDto();
    }
}
