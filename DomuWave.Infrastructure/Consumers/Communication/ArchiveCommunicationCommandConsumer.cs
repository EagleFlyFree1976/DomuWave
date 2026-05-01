using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class ArchiveCommunicationCommandConsumer : InMemoryConsumerBase<ArchiveCommunicationCommand, bool>
{
    private readonly IUserService _userService;

    public ArchiveCommunicationCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        ArchiveCommunicationCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<Communication>()
            .Where(c => c.Id == command.CommunicationId && !c.IsDeleted)
            .Fetch(c => c.Assembly)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null) return false;

        entity.IsArchived = command.Archive;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        // Quando si archivia una comunicazione di tipo Meeting, l'assemblea collegata passa in Convocata
        if (command.Archive && entity.CommunicationType == "Meeting" && entity.Assembly != null)
        {
            var assembly = entity.Assembly;
            if (assembly.Status?.Id == AssemblyStatusLookup.Pianificata)
            {
                var convocataStatus = await session.GetAsync<AssemblyStatusLookup>(AssemblyStatusLookup.Convocata, cancellationToken).ConfigureAwait(false)!;
                assembly.Status = convocataStatus;
                assembly.Trace(currentUser);
                await session.UpdateAsync(assembly, cancellationToken).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
