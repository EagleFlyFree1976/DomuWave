using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteAssemblyCommandConsumer : InMemoryConsumerBase<DeleteAssemblyCommand, bool>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public DeleteAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<bool> Consume(
        DeleteAssemblyCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _assemblyService.GetByIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false);
        if (entity == null) return false;
        entity.SoftDelete(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
