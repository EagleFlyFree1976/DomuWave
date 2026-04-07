using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOpeningBalance;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SetUnitOpeningBalancesBulkCommandConsumer
    : InMemoryConsumerBase<SetUnitOpeningBalancesBulkCommand, bool>
{
    private readonly IUserService _userService;

    public SetUnitOpeningBalancesBulkCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        SetUnitOpeningBalancesBulkCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.FiscalYearId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null)
            throw new NotFoundException("Esercizio non trovato.");

        if (fiscalYear.Status?.Id == FiscalYearStatus.Closed || fiscalYear.Status?.Id == FiscalYearStatus.Locked)
            throw new ValidatorException("Non è possibile modificare il bilancio: l'esercizio è già chiuso.");

        var isFirstFiscalYear = !await session.Query<FiscalYear>()
            .AnyAsync(f => f.Condominium.Id == fiscalYear.Condominium.Id
                        && f.Id != command.Dto.FiscalYearId
                        && f.EndDate < fiscalYear.StartDate
                        && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!isFirstFiscalYear)
            throw new ValidatorException(
                "Il bilancio di apertura non è modificabile: viene propagato automaticamente dal saldo di chiusura dell'esercizio precedente.");

        var unitIds = command.Dto.Items.Select(i => i.UnitId).ToList();

        var units = await session.Query<RealEstateUnit>()
            .Where(u => unitIds.Contains(u.Id) && !u.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingRecords = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == command.Dto.FiscalYearId
                     && unitIds.Contains(b.Unit.Id)
                     && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordByUnitId = existingRecords.ToDictionary(r => r.Unit.Id);
        var unitById       = units.ToDictionary(u => u.Id);

        foreach (var item in command.Dto.Items)
        {
            if (!unitById.TryGetValue(item.UnitId, out var unit)) continue;

            if (!recordByUnitId.TryGetValue(item.UnitId, out var record))
            {
                record = new Models.UnitOpeningBalance
                {
                    Unit       = unit,
                    FiscalYear = fiscalYear,
                    Tenant     = unit.Tenant,
                    IsDeleted  = false,
                };
            
            }
            record.Trace(currentUser);
            record.OpeningBalance = item.OpeningBalance;
            record.Notes          = item.Notes;

            await session.SaveOrUpdateAsync(record, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
