using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteCondominiumCommandConsumer : InMemoryConsumerBase<DeleteCondominiumCommand, bool>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public DeleteCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        DeleteCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _condominiumService
            .DeleteAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
