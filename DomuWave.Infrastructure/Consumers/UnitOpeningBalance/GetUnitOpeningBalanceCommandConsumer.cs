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

public class GetUnitOpeningBalanceCommandConsumer
    : InMemoryConsumerBase<GetUnitOpeningBalanceCommand, UnitOpeningBalanceReadDto>
{
    private readonly IUserService _userService;

    public GetUnitOpeningBalanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<UnitOpeningBalanceReadDto> Consume(
        GetUnitOpeningBalanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var unit = await session.Query<RealEstateUnit>()
            .FirstOrDefaultAsync(x => x.Id == command.UnitId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (unit == null) return null;

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(x => x.Id == command.FiscalYearId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null) return null;

        // Esercizio chiuso?
        var isClosed = fiscalYear.Status?.Id == FiscalYearStatus.Closed
                    || fiscalYear.Status?.Id == FiscalYearStatus.Locked;

        // È il primo esercizio del condominio (nessun esercizio precedente con EndDate < StartDate)?
        var isFirstFiscalYear = !await session.Query<FiscalYear>()
            .AnyAsync(f => f.Condominium.Id == unit.Condominium.Id
                        && f.Id != command.FiscalYearId
                        && f.EndDate < fiscalYear.StartDate
                        && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        // Modificabile solo se: primo esercizio AND non ancora chiuso
        var isEditable = isFirstFiscalYear && !isClosed;

        // Leggi record esistente
        var record = await session.Query<Models.UnitOpeningBalance>()
            .FirstOrDefaultAsync(x => x.Unit.Id == command.UnitId
                                   && x.FiscalYear.Id == command.FiscalYearId
                                   && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (record == null)
        {
            return new UnitOpeningBalanceReadDto
            {
                Id             = 0,
                UnitId         = command.UnitId,
                UnitName       = unit.DisplayName ?? unit.InternalNumber,
                FiscalYearId   = command.FiscalYearId,
                FiscalYearCode = fiscalYear.Code,
                OpeningBalance = 0,
                TotalMovements = 0,
                ClosingBalance = 0,
                Notes          = null,
                IsEditable     = isEditable,
                IsClosed       = isClosed,
            };
        }

        var dto = new UnitOpeningBalanceReadDto
        {
            Id             = record.Id,
            UnitId         = command.UnitId,
            UnitName       = unit.DisplayName ?? unit.InternalNumber,
            FiscalYearId   = command.FiscalYearId,
            FiscalYearCode = fiscalYear.Code,
            OpeningBalance = record.OpeningBalance,
            TotalMovements = record.TotalMovements,
            ClosingBalance = record.ClosingBalance,
            Notes          = record.Notes,
            IsEditable     = isEditable,
            IsClosed       = isClosed,
        };
        dto.SetTraceInfo(record);
        return dto;
    }
}
