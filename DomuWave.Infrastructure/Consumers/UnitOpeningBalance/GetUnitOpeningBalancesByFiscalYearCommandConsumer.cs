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

        // Tutti i saldi già salvati per questo esercizio
        var records = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == command.FiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recordByUnitId = records.ToDictionary(r => r.Unit.Id);

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

        return units
            .OrderBy(u => u.FormatUnitName())
            .Select(u =>
            {
                recordByUnitId.TryGetValue(u.Id, out var rec);
                var dto = new UnitOpeningBalanceReadDto
                {
                    Id              = rec?.Id ?? 0,
                    UnitId          = u.Id,
                    UnitName        = u.FormatUnitName(),
                    FiscalYearId    = command.FiscalYearId,
                    FiscalYearCode  = fiscalYear.Code,
                    BillingGroupId   = u.BillingGroup?.Id,
                    BillingGroupName = u.BillingGroup?.Name,
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
                return dto;
            })
            .ToList();
    }
}
