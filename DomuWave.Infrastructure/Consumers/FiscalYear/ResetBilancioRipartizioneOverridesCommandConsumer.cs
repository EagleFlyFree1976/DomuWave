using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

/// <summary>
/// "Ripristina automatico": soft-cancella tutti gli override del bilancio di ripartizione
/// dell'esercizio. Ritorna il report ricalcolato dai dati.
/// </summary>
public class ResetBilancioRipartizioneOverridesCommandConsumer
    : InMemoryConsumerBase<ResetBilancioRipartizioneOverridesCommand, BilancioRipartizioneReportDto>
{
    private readonly IUserService _userService;
    private readonly IMediator    _mediator;

    public ResetBilancioRipartizioneOverridesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IMediator               mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _mediator    = mediator;
    }

    protected override async Task<BilancioRipartizioneReportDto> Consume(
        ResetBilancioRipartizioneOverridesCommand command,
        IMediationContext                         mediationContext,
        CancellationToken                         cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fy = await session.Query<Models.FiscalYear>()
            .FirstOrDefaultAsync(f => f.Id == command.FiscalYearId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (fy == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        if (fy.Status?.Id == FiscalYearStatus.Closed || fy.Status?.Id == FiscalYearStatus.Locked)
            throw new ValidatorException("Il bilancio non è modificabile: l'esercizio è chiuso.");

        var existing = await session.Query<BilancioRipartizioneOverride>()
            .Where(o => o.FiscalYear.Id == fy.Id && !o.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var old in existing)
        {
            old.IsDeleted = true;
            old.Trace(currentUser);
            await session.SaveOrUpdateAsync(old, cancellationToken).ConfigureAwait(false);
        }

        if (existing.Any())
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator
            .GetResponse(new GetBilancioRipartizioneReportCommand(command.CurrentUserId, fy.Id), cancellationToken)
            .ConfigureAwait(false);
    }
}
