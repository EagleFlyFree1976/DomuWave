using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class DeleteQuoteCommandConsumer
    : InMemoryConsumerBase<DeleteQuoteCommand, bool>
{
    private readonly IWorkQuoteService _quoteService;
    private readonly IUserService      _userService;

    public DeleteQuoteCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IWorkQuoteService quoteService,
        IUserService userService) : base(sessionFactoryProvider)
    { _quoteService = quoteService; _userService = userService; }

    protected override async Task<bool> Consume(
        DeleteQuoteCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return await _quoteService.DeleteAsync(command.QuoteId, currentUser, cancellationToken).ConfigureAwait(false);
    }
}
