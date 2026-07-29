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

public class GetGroupOpeningBalanceCommandConsumer
    : InMemoryConsumerBase<GetGroupOpeningBalanceCommand, UnitOpeningBalanceReadDto>
{
    private readonly IUserService _userService;

    public GetGroupOpeningBalanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<UnitOpeningBalanceReadDto> Consume(
        GetGroupOpeningBalanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var group = await session.Query<BillingGroup>()
            .FirstOrDefaultAsync(g => g.Id == command.BillingGroupId && !g.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (group == null) return null;

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(x => x.Id == command.FiscalYearId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null) return null;

        var isClosed = fiscalYear.Status?.Id == FiscalYearStatus.Closed
                    || fiscalYear.Status?.Id == FiscalYearStatus.Locked;

        var isFirstFiscalYear = fiscalYear.PreviousFiscalYear == null;

        var isEditable = isFirstFiscalYear && !isClosed;

        var record = await session.Query<Models.BillingGroupOpeningBalance>()
            .FirstOrDefaultAsync(x => x.BillingGroup.Id == command.BillingGroupId
                                   && x.FiscalYear.Id == command.FiscalYearId
                                   && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (record == null)
        {
            return new UnitOpeningBalanceReadDto
            {
                Id               = 0,
                UnitId           = 0,
                UnitName         = group.Name,
                FiscalYearId     = command.FiscalYearId,
                FiscalYearCode   = fiscalYear.Code,
                IsGroup          = true,
                BillingGroupId   = group.Id,
                BillingGroupName = group.Name,
                OpeningBalance   = 0,
                RateAddebitate   = 0,
                RateIncassate    = 0,
                QuotaConsuntiva  = 0,
                SaldoConguaglio  = 0,
                TotalMovements   = 0,
                ClosingBalance   = 0,
                Notes            = null,
                IsEditable       = isEditable,
                IsClosed         = isClosed,
            };
        }

        var dto = new UnitOpeningBalanceReadDto
        {
            Id               = record.Id,
            UnitId           = 0,
            UnitName         = group.Name,
            FiscalYearId     = command.FiscalYearId,
            FiscalYearCode   = fiscalYear.Code,
            IsGroup          = true,
            BillingGroupId   = group.Id,
            BillingGroupName = group.Name,
            OpeningBalance   = record.OpeningBalance,
            RateAddebitate   = record.RateAddebitate,
            RateIncassate    = record.RateIncassate,
            QuotaConsuntiva  = record.QuotaConsuntiva,
            SaldoConguaglio  = record.SaldoConguaglio,
            TotalMovements   = record.TotalMovements,
            ClosingBalance   = record.ClosingBalance,
            Notes            = record.Notes,
            IsEditable       = isEditable,
            IsClosed         = isClosed,
        };
        dto.SetTraceInfo(record);
        return dto;
    }
}
