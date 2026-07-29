using DomuWave.Services.Interfaces.Extensions;
using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOpeningBalance;
using DomuWave.Services.Dto.UnitOpeningBalance;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetUnitOpeningBalancesByFiscalYearCommandConsumer
    : InMemoryConsumerBase<GetUnitOpeningBalancesByFiscalYearCommand, IList<UnitOpeningBalanceReadDto>>
{
    private readonly IUserService _userService;

    public GetUnitOpeningBalancesByFiscalYearCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<UnitOpeningBalanceReadDto>> Consume(
        GetUnitOpeningBalancesByFiscalYearCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(x => x.Id == command.FiscalYearId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null) return [];

        var isClosed = fiscalYear.Status?.Id == FiscalYearStatus.Closed
                    || fiscalYear.Status?.Id == FiscalYearStatus.Locked;

        var isFirstFiscalYear = fiscalYear.PreviousFiscalYear == null;

        var isEditable = isFirstFiscalYear && !isClosed;

        // Tutte le unità attive del condominio
        var units = await session.Query<RealEstateUnit>()
            .Where(u => u.Condominium.Id == fiscalYear.Condominium.Id && u.IsActive && !u.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Tutti i saldi unità già salvati per questo esercizio
        var unitRecords = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == command.FiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordByUnitId = unitRecords.ToDictionary(r => r.Unit.Id);

        // Tutti i saldi gruppo già salvati per questo esercizio
        var groupRecords = await session.Query<Models.BillingGroupOpeningBalance>()
            .Where(b => b.FiscalYear.Id == command.FiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordByGroupId = groupRecords.ToDictionary(r => r.BillingGroup.Id);

        // Millesimi della tabella principale del condominio (per la ripartizione lato client)
        var millesimalRows = await session.Query<UnitMillesimal>()
            .Where(um => um.MillesimalTable.Condominium.Id == fiscalYear.Condominium.Id
                      && um.MillesimalTable.IsDefault
                      && !um.MillesimalTable.IsDeleted
                      && !um.IsDeleted)
            .Select(um => new { UnitId = um.Unit.Id, um.Millesimal })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var millesimalByUnitId = millesimalRows.ToDictionary(um => um.UnitId, um => um.Millesimal);

        var result = new List<UnitOpeningBalanceReadDto>();

        // Unità senza gruppo → riga individuale
        foreach (var u in units.Where(u => u.BillingGroup == null).OrderBy(u => u.FormatUnitName()))
        {
            recordByUnitId.TryGetValue(u.Id, out var rec);
            var dto = new UnitOpeningBalanceReadDto
            {
                Id              = rec?.Id ?? 0,
                UnitId          = u.Id,
                UnitName        = u.FormatUnitName(),
                FiscalYearId    = command.FiscalYearId,
                FiscalYearCode  = fiscalYear.Code,
                Millesimal       = millesimalByUnitId.TryGetValue(u.Id, out var mil) ? mil : 0,
                OpeningBalance  = rec?.OpeningBalance  ?? 0,
                RateAddebitate  = rec?.RateAddebitate  ?? 0,
                RateIncassate   = rec?.RateIncassate   ?? 0,
                QuotaConsuntiva = rec?.QuotaConsuntiva ?? 0,
                SaldoConguaglio = rec?.SaldoConguaglio ?? 0,
                TotalMovements  = rec?.TotalMovements  ?? 0,
                ClosingBalance  = rec?.ClosingBalance  ?? 0,
                Notes           = rec?.Notes,
                IsEditable      = isEditable,
                IsClosed        = isClosed,
            };
            if (rec != null) dto.SetTraceInfo(rec);
            result.Add(dto);
        }

        // Unità con gruppo → una riga aggregata per gruppo (mai spalmata sulle unità componenti)
        var unitsByGroup = units
            .Where(u => u.BillingGroup != null)
            .GroupBy(u => u.BillingGroup);

        foreach (var grp in unitsByGroup.OrderBy(g => g.Key.Name))
        {
            var group = grp.Key;
            recordByGroupId.TryGetValue(group.Id, out var rec);
            var groupMillesimal = grp.Sum(u => millesimalByUnitId.TryGetValue(u.Id, out var mil) ? mil : 0);

            var dto = new UnitOpeningBalanceReadDto
            {
                Id               = rec?.Id ?? 0,
                UnitId           = 0,
                UnitName         = group.Name,
                FiscalYearId     = command.FiscalYearId,
                FiscalYearCode   = fiscalYear.Code,
                IsGroup          = true,
                BillingGroupId   = group.Id,
                BillingGroupName = group.Name,
                Millesimal       = groupMillesimal,
                OpeningBalance   = rec?.OpeningBalance  ?? 0,
                RateAddebitate   = rec?.RateAddebitate  ?? 0,
                RateIncassate    = rec?.RateIncassate   ?? 0,
                QuotaConsuntiva  = rec?.QuotaConsuntiva ?? 0,
                SaldoConguaglio  = rec?.SaldoConguaglio ?? 0,
                TotalMovements   = rec?.TotalMovements  ?? 0,
                ClosingBalance   = rec?.ClosingBalance  ?? 0,
                Notes            = rec?.Notes,
                IsEditable       = isEditable,
                IsClosed         = isClosed,
            };
            if (rec != null) dto.SetTraceInfo(rec);
            result.Add(dto);
        }

        return result;
    }
}
