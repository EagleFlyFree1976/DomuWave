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

public class GetCommunicationsByTypeCommandConsumer : InMemoryConsumerBase<GetCommunicationsByTypeCommand, IList<CommunicationReadDto>>
{
    private readonly IUserService _userService;

    public GetCommunicationsByTypeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<CommunicationReadDto>> Consume(
        GetCommunicationsByTypeCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<Communication>()
            .Where(c => c.Condominium.Id == command.CondominiumId
                     && c.CommunicationType == command.CommunicationType
                     && !c.IsDeleted)
            .OrderByDescending(c => c.PublicationDate)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return list.Select(c => c.ToReadDto()).ToList();
    }
}
