using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Dto.Assembly;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class OpenAssemblyCommandConsumer : InMemoryConsumerBase<OpenAssemblyCommand, AssemblyReadDto>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public OpenAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<AssemblyReadDto> Consume(
        OpenAssemblyCommand command,
        IMediationContext    mediationContext,
        CancellationToken   cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _assemblyService.GetByIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Assemblea non trovata.");

        if (entity.Status?.Id != AssemblyStatusLookup.Convocata)
            throw new ValidatorException("Solo le assemblee in stato 'Convocata' possono essere aperte.");

        var today = DateTime.UtcNow.Date;
        if (entity.ScheduledDate.Date != today)
            throw new ValidatorException("L'assemblea può essere aperta solo nel giorno programmato.");

        var status = await session.GetAsync<AssemblyStatusLookup>(AssemblyStatusLookup.InCorso, cancellationToken).ConfigureAwait(false)!;
        entity.Status = status;
        entity.Trace(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
