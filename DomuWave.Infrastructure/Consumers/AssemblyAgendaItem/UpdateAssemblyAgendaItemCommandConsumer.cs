using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AssemblyAgendaItem;
using DomuWave.Services.Dto.AssemblyAgendaItem;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateAssemblyAgendaItemCommandConsumer : InMemoryConsumerBase<UpdateAssemblyAgendaItemCommand, AssemblyAgendaItemReadDto>
{
    private readonly IAssemblyAgendaItemService _agendaItemService;
    private readonly IUserService               _userService;

    public UpdateAssemblyAgendaItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyAgendaItemService agendaItemService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _agendaItemService = agendaItemService;
        _userService       = userService;
    }

    protected override async Task<AssemblyAgendaItemReadDto> Consume(
        UpdateAssemblyAgendaItemCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _agendaItemService.GetByIdAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Punto OdG non trovato.");

        AgendaItemVoteResultLookup? voteResult = null;
        if (command.Dto.VoteResultId.HasValue)
            voteResult = await session.GetAsync<AgendaItemVoteResultLookup>(command.Dto.VoteResultId.Value, cancellationToken).ConfigureAwait(false);

        entity.ApplyUpdate(command.Dto, voteResult);
        entity.Trace(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
