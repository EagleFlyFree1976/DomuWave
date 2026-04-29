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

public class GetUnreadCommunicationsCommandConsumer : InMemoryConsumerBase<GetUnreadCommunicationsCommand, IList<CommunicationReadDto>>
{
    private readonly IUserService _userService;

    public GetUnreadCommunicationsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<CommunicationReadDto>> Consume(
        GetUnreadCommunicationsCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var readIds = await session.Query<CommunicationRead>()
            .Where(r => r.Communication.Condominium.Id == command.CondominiumId && r.UserId == command.UserId)
            .Select(r => r.Communication.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var list = await session.Query<Communication>()
            .Where(c => c.Condominium.Id == command.CondominiumId
                     && c.IsVisible && !c.IsDeleted
                     && !readIds.Contains(c.Id))
            .OrderByDescending(c => c.PublicationDate)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return list.Select(c => c.ToReadDto()).ToList();
    }
}
