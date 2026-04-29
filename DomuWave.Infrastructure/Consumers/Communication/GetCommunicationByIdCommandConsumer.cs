using CPQ.Core.Consumers;
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

public class GetCommunicationByIdCommandConsumer : InMemoryConsumerBase<GetCommunicationByIdCommand, CommunicationReadDto?>
{
    private readonly IUserService _userService;

    public GetCommunicationByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<CommunicationReadDto?> Consume(
        GetCommunicationByIdCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<Communication>()
            .Where(c => c.Id == command.CommunicationId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return entity?.ToReadDto();
    }
}
