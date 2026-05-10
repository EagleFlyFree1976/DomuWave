using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Staircase;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteStaircaseCommandConsumer : InMemoryConsumerBase<DeleteStaircaseCommand, bool>
{
    private readonly IStaircaseService _staircaseService;
    private readonly IUserService      _userService;

    public DeleteStaircaseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IStaircaseService staircaseService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _staircaseService = staircaseService;
        _userService      = userService;
    }

    protected override async Task<bool> Consume(
        DeleteStaircaseCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _staircaseService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
