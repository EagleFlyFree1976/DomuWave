using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using CPQ.Core.Startups;
using Microsoft.Extensions.Logging;
using NHibernate;

namespace DomuWave.Services.Jobs;

public class TruncateOldLogJob : ServiceBase, IRecurrenceAsyncJob
{
    private readonly ISessionFactoryProvider _sessionFactoryProvider;
    private readonly ILogger<TruncateOldLogJob> _logger;

    public TruncateOldLogJob(ISessionFactoryProvider sessionFactoryProvider, ILogger<TruncateOldLogJob> logger) : base(sessionFactoryProvider)
    {
        _logger = logger;
        _sessionFactoryProvider = sessionFactoryProvider;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[TruncateOldLogJob] Avvio sp_domus_truncateoldlog");

        using var tx = session.BeginTransaction();
        await session.CreateSQLQuery("EXEC sp_domus_truncateoldlog")
            .ExecuteUpdateAsync(cancellationToken)
            .ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("[TruncateOldLogJob] sp_domus_truncateoldlog completata");
    }
}
