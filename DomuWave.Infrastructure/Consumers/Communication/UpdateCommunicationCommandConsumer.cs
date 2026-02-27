using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateCommunicationCommandConsumer : InMemoryConsumerBase<UpdateCommunicationCommand, Communication>
{
    private readonly ICommunicationService _communicationService;
    private readonly IUserService _userService;

    public UpdateCommunicationCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICommunicationService communicationService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _communicationService = communicationService;
        _userService = userService;
    }

    protected override async Task<Communication> Consume(
        UpdateCommunicationCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _communicationService
            .GetByIdAsync(command.CommunicationId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;
        command.Entity.Id = command.CommunicationId;
        return await _communicationService
            .UpdateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
