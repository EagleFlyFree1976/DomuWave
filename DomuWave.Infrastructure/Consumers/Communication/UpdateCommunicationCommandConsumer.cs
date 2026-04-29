using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Dto.Communication;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateCommunicationCommandConsumer : InMemoryConsumerBase<UpdateCommunicationCommand, CommunicationReadDto?>
{
    private readonly IUserService _userService;

    public UpdateCommunicationCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<CommunicationReadDto?> Consume(
        UpdateCommunicationCommand command,
        IMediationContext           mediationContext,
        CancellationToken          cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<Communication>()
            .Where(c => c.Id == command.Id && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null) return null;

        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
