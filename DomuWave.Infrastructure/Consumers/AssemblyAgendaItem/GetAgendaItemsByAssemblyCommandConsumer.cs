using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AssemblyAgendaItem;
using DomuWave.Services.Dto.AssemblyAgendaItem;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetAgendaItemsByAssemblyCommandConsumer : InMemoryConsumerBase<GetAgendaItemsByAssemblyCommand, IList<AssemblyAgendaItemReadDto>>
{
    private readonly IAssemblyAgendaItemService _agendaItemService;
    private readonly IUserService               _userService;

    public GetAgendaItemsByAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyAgendaItemService agendaItemService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _agendaItemService = agendaItemService;
        _userService       = userService;
    }

    protected override async Task<IList<AssemblyAgendaItemReadDto>> Consume(
        GetAgendaItemsByAssemblyCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var items       = await _agendaItemService.GetByAssemblyIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false);
        return items.Select(i => i.ToReadDto()).ToList();
    }
}
