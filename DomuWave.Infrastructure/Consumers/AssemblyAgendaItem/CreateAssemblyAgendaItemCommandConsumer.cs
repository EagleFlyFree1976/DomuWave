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

public class CreateAssemblyAgendaItemCommandConsumer : InMemoryConsumerBase<CreateAssemblyAgendaItemCommand, AssemblyAgendaItemReadDto>
{
    private readonly IUserService _userService;

    public CreateAssemblyAgendaItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<AssemblyAgendaItemReadDto> Consume(
        CreateAssemblyAgendaItemCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo del punto è obbligatorio.");

        var assembly = await session.GetAsync<Models.Assembly>(command.Dto.AssemblyId, cancellationToken).ConfigureAwait(false)
                       ?? throw new NotFoundException("Assemblea non trovata.");

        var voteResult = await session.GetAsync<AgendaItemVoteResultLookup>(AgendaItemVoteResultLookup.NonVotato, cancellationToken).ConfigureAwait(false)!;

        var entity = command.Dto.ToEntity(assembly, assembly.Tenant, voteResult);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
