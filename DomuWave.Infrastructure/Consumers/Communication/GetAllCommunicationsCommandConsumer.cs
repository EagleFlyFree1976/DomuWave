using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAllCommunicationsCommandConsumer : InMemoryConsumerBase<GetAllCommunicationsCommand, IList<Models.Communication>>
{
    private readonly ICommunicationService _communicationService;
    private readonly IUserService _userService;

    public GetAllCommunicationsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICommunicationService communicationService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _communicationService = communicationService;
        _userService = userService;
    }

    protected override async Task<IList<Models.Communication>> Consume(
        GetAllCommunicationsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _communicationService
            .GetAllAsync(currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
