using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AssemblyAgendaItem;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteAssemblyAgendaItemCommandConsumer : InMemoryConsumerBase<DeleteAssemblyAgendaItemCommand, bool>
{
    private readonly IAssemblyAgendaItemService _agendaItemService;
    private readonly IUserService               _userService;

    public DeleteAssemblyAgendaItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyAgendaItemService agendaItemService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _agendaItemService = agendaItemService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        DeleteAssemblyAgendaItemCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _agendaItemService.GetByIdAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false);
        if (entity == null) return false;
        entity.SoftDelete(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
