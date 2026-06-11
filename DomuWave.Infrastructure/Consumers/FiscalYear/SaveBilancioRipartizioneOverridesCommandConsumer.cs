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
/// Salva in blocco gli override delle celle del bilancio di ripartizione: soft-cancella il
/// set precedente e reinserisce le celle ricevute. Consentito solo a esercizio non
/// Closed/Locked. Ritorna il report ricalcolato.
/// </summary>
public class SaveBilancioRipartizioneOverridesCommandConsumer
    : InMemoryConsumerBase<SaveBilancioRipartizioneOverridesCommand, BilancioRipartizioneReportDto>
{
    private readonly IUserService _userService;
    private readonly IMediator    _mediator;

    public SaveBilancioRipartizioneOverridesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IMediator               mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _mediator    = mediator;
    }

    protected override async Task<BilancioRipartizioneReportDto> Consume(
        SaveBilancioRipartizioneOverridesCommand command,
        IMediationContext                        mediationContext,
        CancellationToken                        cancellationToken)
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

        // Celle valide ricevute, consolidando eventuali duplicati (ultimo vince).
        var deduped = command.Cells
            .Where(c => c.UnitId > 0
                     && c.RowType  is BilancioRowType.Proprietari or BilancioRowType.Inquilini
                     && c.CellType is >= BilancioCellType.Consumo and <= BilancioCellType.Versato)
            .GroupBy(c => (c.UnitId, c.RowType, c.CellType, c.ColumnRefId))
            .ToDictionary(g => g.Key, g => g.Last());

        // ── 1. Carica TUTTI i record esistenti (anche soft-deleted) per evitare
        //       violazioni dell'indice univoco filtrato (WHERE IsDeleted = 0). ──────
        var existingAll = await session.Query<BilancioRipartizioneOverride>()
            .Where(o => o.FiscalYear.Id == fy.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Per ogni chiave-cella tieni un solo record (preferendo quello attivo) e
        // soft-cancella eventuali duplicati, così l'upsert non collide con l'indice.
        var existingByKey = new Dictionary<(int, int, int, int), BilancioRipartizioneOverride>();
        foreach (var grp in existingAll.GroupBy(o => (o.Unit.Id, o.RowType, o.CellType, o.ColumnRefId)))
        {
            var keep = grp.FirstOrDefault(o => !o.IsDeleted) ?? grp.First();
            existingByKey[grp.Key] = keep;
            foreach (var dup in grp.Where(o => o != keep && !o.IsDeleted))
            {
                dup.IsDeleted = true;
                dup.Trace(currentUser);
                await session.SaveOrUpdateAsync(dup, cancellationToken).ConfigureAwait(false);
            }
        }

        // ── 2. Soft-delete delle celle non più presenti nel nuovo set ────────────
        foreach (var kv in existingByKey)
        {
            if (!deduped.ContainsKey(kv.Key) && !kv.Value.IsDeleted)
            {
                kv.Value.IsDeleted = true;
                kv.Value.Trace(currentUser);
                await session.SaveOrUpdateAsync(kv.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        // Flush del soft-delete PRIMA degli insert/riattivazioni: l'indice univoco
        // filtrato vedrebbe altrimenti due righe attive con la stessa chiave nello
        // stesso batch (stesso pattern di ExpenseAllocationHelper).
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // ── 3. Upsert delle celle ricevute (riusa il record esistente se c'è) ────
        foreach (var (key, cell) in deduped)
        {
            if (existingByKey.TryGetValue(key, out var entity))
            {
                entity.IsDeleted = false;
                entity.Amount    = cell.Amount;
                entity.Trace(currentUser);
                await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var unit = session.Load<RealEstateUnit>(cell.UnitId);
                var created = new BilancioRipartizioneOverride
                {
                    FiscalYear  = fy,
                    Unit        = unit,
                    Tenant      = fy.Tenant,
                    RowType     = cell.RowType,
                    CellType    = cell.CellType,
                    ColumnRefId = cell.ColumnRefId,
                    Amount      = cell.Amount,
                };
                created.Trace(currentUser);
                await session.SaveOrUpdateAsync(created, cancellationToken).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // ── 3. Report ricalcolato (con override appena salvati) ──────────────────
        return await _mediator
            .GetResponse(new GetBilancioRipartizioneReportCommand(command.CurrentUserId, fy.Id), cancellationToken)
            .ConfigureAwait(false);
    }
}
