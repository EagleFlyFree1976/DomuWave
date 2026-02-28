using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteUnitOwnerCommandConsumer : InMemoryConsumerBase<DeleteUnitOwnerCommand, bool>
{
    private readonly IUnitOwnerService  _unitOwnerService;
    private readonly IUserService       _userService;

    public DeleteUnitOwnerCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitOwnerService unitOwnerService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _unitOwnerService = unitOwnerService;
        _userService      = userService;
    }

    protected override async Task<bool> Consume(
        DeleteUnitOwnerCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _unitOwnerService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
