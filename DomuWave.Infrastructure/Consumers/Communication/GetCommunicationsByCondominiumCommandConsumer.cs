using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;
using DomuWave.Services.Models;
namespace DomuWave.Services.Consumers;

public class GetCommunicationsByCondominiumCommandConsumer : InMemoryConsumerBase<GetCommunicationsByCondominiumCommand, IList<Models.Communication>>
{
    private readonly ICommunicationService _communicationService;
    private readonly IUserService _userService;

    public GetCommunicationsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICommunicationService communicationService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _communicationService = communicationService;
        _userService = userService;
    }

    protected override async Task<IList<Models.Communication>> Consume(
        GetCommunicationsByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _communicationService
            .GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
