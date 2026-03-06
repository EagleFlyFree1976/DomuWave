using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitMillesimal;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteUnitMillesimalCommandConsumer
    : InMemoryConsumerBase<DeleteUnitMillesimalCommand, bool>
{
    private readonly IUnitMillesimalService _unitMillesimalService;
    private readonly IUserService           _userService;

    public DeleteUnitMillesimalCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitMillesimalService  unitMillesimalService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _unitMillesimalService = unitMillesimalService;
        _userService           = userService;
    }

    protected override async Task<bool> Consume(
        DeleteUnitMillesimalCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _unitMillesimalService
            .DeleteAsync(command.EntryId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
