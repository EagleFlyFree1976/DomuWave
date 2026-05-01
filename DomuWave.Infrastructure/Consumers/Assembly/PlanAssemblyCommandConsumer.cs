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

public class PlanAssemblyCommandConsumer : InMemoryConsumerBase<PlanAssemblyCommand, AssemblyReadDto>
{
    private readonly IAssemblyService _assemblyService;
    private readonly IUserService     _userService;

    public PlanAssemblyCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IAssemblyService assemblyService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
    }

    protected override async Task<AssemblyReadDto> Consume(
        PlanAssemblyCommand command,
        IMediationContext    mediationContext,
        CancellationToken   cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _assemblyService.GetByIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Assemblea non trovata.");

        if (entity.Status?.Id != AssemblyStatusLookup.Bozza)
            throw new ValidatorException("Solo le assemblee in stato 'Bozza' possono essere spostate in 'Pianificata'.");

        var status = await session.GetAsync<AssemblyStatusLookup>(AssemblyStatusLookup.Pianificata, cancellationToken).ConfigureAwait(false)!;
        entity.Status = status;
        entity.Trace(currentUser);

        // Crea automaticamente la comunicazione di convocazione
        var communication = new Communication
        {
            Condominium       = entity.Condominium,
            Tenant            = entity.Tenant,
            Assembly          = entity,
            Name              = $"Convocazione: {entity.Name}",
            Description       = entity.Notes ?? string.Empty,
            CommunicationType = "Meeting",
            Priority          = "Normal",
            PublicationDate   = DateTime.UtcNow,
            ExpirationDate    = entity.ScheduledDate,
            SendEmail         = false,
            IsVisible         = false,
            IsArchived        = false,
        };
        communication.Trace(currentUser);
        await session.SaveAsync(communication, cancellationToken).ConfigureAwait(false);

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
