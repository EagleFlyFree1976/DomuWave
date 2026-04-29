using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteCommunicationCommandConsumer : InMemoryConsumerBase<DeleteCommunicationCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteCommunicationCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        DeleteCommunicationCommand command,
        IMediationContext           mediationContext,
        CancellationToken          cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<Communication>()
            .Where(c => c.Id == command.CommunicationId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
