using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetVisibleCommunicationsCommandConsumer : InMemoryConsumerBase<GetVisibleCommunicationsCommand, IList<Communication>>
{
    private readonly ICommunicationService _communicationService;
    private readonly IUserService _userService;

    public GetVisibleCommunicationsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICommunicationService communicationService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _communicationService = communicationService;
        _userService = userService;
    }

    protected override async Task<IList<Communication>> Consume(
        GetVisibleCommunicationsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _communicationService
            .GetVisibleCommunicationsAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
