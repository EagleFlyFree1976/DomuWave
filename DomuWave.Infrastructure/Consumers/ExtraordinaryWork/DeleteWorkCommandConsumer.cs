using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class DeleteWorkCommandConsumer
    : InMemoryConsumerBase<DeleteWorkCommand, bool>
{
    private readonly IExtraordinaryWorkService _workService;
    private readonly IUserService              _userService;

    public DeleteWorkCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExtraordinaryWorkService workService,
        IUserService userService) : base(sessionFactoryProvider)
    { _workService = workService; _userService = userService; }

    protected override async Task<bool> Consume(
        DeleteWorkCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return await _workService.DeleteAsync(command.WorkId, currentUser, cancellationToken).ConfigureAwait(false);
    }
}
