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

public class GetCommunicationsByCondominiumCommandConsumer : InMemoryConsumerBase<GetCommunicationsByCondominiumCommand, IList<CommunicationReadDto>>
{
    private readonly IUserService       _userService;
    private readonly IUserTenantService _userTenantService;

    public GetCommunicationsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IUserTenantService      userTenantService) : base(sessionFactoryProvider)
    {
        _userService       = userService;
        _userTenantService = userTenantService;
    }

    protected override async Task<IList<CommunicationReadDto>> Consume(
        GetCommunicationsByCondominiumCommand command,
        IMediationContext                      mediationContext,
        CancellationToken                     cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var query = session.Query<Communication>()
            .Where(c => c.Condominium.Id == command.CondominiumId && !c.IsDeleted);

        var isCondomino = await _userTenantService
            .IsCondominoInCondominiumAsync(command.CurrentUserId, command.CondominiumId, cancellationToken)
            .ConfigureAwait(false);
        if (isCondomino)
        {
            query = query.Where(c => c.IsVisible && !c.IsArchived);
        }
        else
        {
            query = command.IncludeArchived
                ? query.Where(c => c.IsArchived)
                : query.Where(c => !c.IsArchived);
        }

        var list = await query
            .OrderByDescending(c => c.PublicationDate)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var communicationIds = list.Select(c => c.Id).ToList();

        var sentSet = await session.Query<CommunicationNotification>()
            .Where(n => communicationIds.Contains(n.Communication.Id) && !n.IsDeleted && n.Status >= 2)
            .Select(n => n.Communication.Id)
            .Distinct()
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var sentIds = new HashSet<int>(sentSet);

        return list.Select(c =>
        {
            var dto = c.ToReadDto();
            dto.HasSentNotifications = sentIds.Contains(c.Id);
            return dto;
        }).ToList();
    }
}
