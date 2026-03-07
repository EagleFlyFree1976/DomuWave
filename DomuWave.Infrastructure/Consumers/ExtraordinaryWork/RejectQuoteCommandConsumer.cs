using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Dto.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class RejectQuoteCommandConsumer
    : InMemoryConsumerBase<RejectQuoteCommand, WorkQuoteReadDto>
{
    private readonly IWorkQuoteService _quoteService;
    private readonly IUserService      _userService;

    public RejectQuoteCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IWorkQuoteService quoteService,
        IUserService userService) : base(sessionFactoryProvider)
    { _quoteService = quoteService; _userService = userService; }

    protected override async Task<WorkQuoteReadDto> Consume(
        RejectQuoteCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var result = await _quoteService.RejectAsync(command.QuoteId, currentUser, cancellationToken).ConfigureAwait(false);
        return result?.ToReadDto();
    }
}
