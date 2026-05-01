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

public class SaveAssemblyMinutesCommandConsumer : InMemoryConsumerBase<SaveAssemblyMinutesCommand, AssemblyReadDto>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public SaveAssemblyMinutesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService        assemblyService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<AssemblyReadDto> Consume(
        SaveAssemblyMinutesCommand command,
        IMediationContext           mediationContext,
        CancellationToken          cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _assemblyService.GetByIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Assemblea non trovata.");

        if (entity.Status?.Id != AssemblyStatusLookup.Convocata && entity.Status?.Id != AssemblyStatusLookup.Svolta)
            throw new ValidatorException("Il verbale può essere redatto solo per assemblee in stato 'Convocata' o 'Svolta'.");

        entity.BoardMembers = command.BoardMembers;
        entity.Minutes      = command.Minutes;
        entity.Trace(currentUser);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
