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

public class SetGroupOpeningBalanceCommandConsumer
    : InMemoryConsumerBase<SetGroupOpeningBalanceCommand, bool>
{
    private readonly IUserService _userService;

    public SetGroupOpeningBalanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<bool> Consume(
        SetGroupOpeningBalanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var group = await session.Query<BillingGroup>()
            .FirstOrDefaultAsync(g => g.Id == command.BillingGroupId && !g.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (group == null)
            throw new NotFoundException("Gruppo di fatturazione non trovato.");

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

        var units = await session.Query<RealEstateUnit>()
            .Where(u => u.BillingGroup.Id == command.BillingGroupId && !u.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (units.Count == 0)
            throw new ValidatorException("Il gruppo non contiene unità immobiliari.");

        var unitIds = units.Select(u => u.Id).ToList();

        var isEqualSplit = string.Equals(command.Dto.Criterion, "equal", StringComparison.OrdinalIgnoreCase);

        Dictionary<int, decimal> weightByUnitId;

        if (isEqualSplit)
        {
            weightByUnitId = unitIds.ToDictionary(id => id, _ => 1m);
        }
        else
        {
            var defaultTable = await session.Query<MillesimalTable>()
                .FirstOrDefaultAsync(t => t.Condominium.Id == fiscalYear.Condominium.Id && t.IsDefault && !t.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (defaultTable == null)
                throw new ValidatorException("Nessuna tabella millesimale principale trovata per il condominio.");

            var unitMillesimals = await session.Query<UnitMillesimal>()
                .Where(um => um.MillesimalTable.Id == defaultTable.Id && unitIds.Contains(um.Unit.Id) && !um.IsDeleted)
                .Select(um => new { UnitId = um.Unit.Id, um.Millesimal })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            weightByUnitId = unitMillesimals.ToDictionary(um => um.UnitId, um => um.Millesimal);

            var missingUnit = units.FirstOrDefault(u => !weightByUnitId.ContainsKey(u.Id));
            if (missingUnit != null)
                throw new ValidatorException(
                    $"L'unità {missingUnit.InternalNumber} non ha un millesimo assegnato nella tabella principale.");
        }

        decimal totalWeight = unitIds.Sum(id => weightByUnitId[id]);
        if (totalWeight <= 0)
            throw new ValidatorException("Il totale dei millesimi delle unità del gruppo è zero: impossibile ripartire l'importo.");

        // Prima passata — arrotonda a 2 decimali
        var rawRows = unitIds
            .Select(id =>
            {
                decimal weight  = weightByUnitId[id];
                decimal raw     = command.Dto.TotalAmount * weight / totalWeight;
                decimal rounded = Math.Round(raw, 2, MidpointRounding.AwayFromZero);
                return new { UnitId = id, Millesimal = weight, Rounded = rounded };
            })
            .ToList();

        // Residuo all'unità con millesimo maggiore
        decimal roundedSum = rawRows.Sum(r => r.Rounded);
        decimal remainder  = Math.Round(command.Dto.TotalAmount - roundedSum, 2, MidpointRounding.AwayFromZero);
        int?    largestId  = rawRows.OrderByDescending(r => r.Millesimal).First().UnitId;

        var existingRecords = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == command.Dto.FiscalYearId
                     && unitIds.Contains(b.Unit.Id)
                     && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordByUnitId = existingRecords.ToDictionary(r => r.Unit.Id);
        var unitById        = units.ToDictionary(u => u.Id);

        foreach (var row in rawRows)
        {
            var unit    = unitById[row.UnitId];
            var amount  = row.Rounded + (row.UnitId == largestId ? remainder : 0m);

            if (!recordByUnitId.TryGetValue(row.UnitId, out var record))
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
            record.OpeningBalance = amount;
            record.Notes          = command.Dto.Notes;

            await session.SaveOrUpdateAsync(record, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
