using DomuWave.Services.Interfaces.Extensions;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOpeningBalance;
using DomuWave.Services.Dto.UnitOpeningBalance;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SetUnitOpeningBalanceCommandConsumer
    : InMemoryConsumerBase<SetUnitOpeningBalanceCommand, UnitOpeningBalanceReadDto>
{
    private readonly IUserService _userService;

    public SetUnitOpeningBalanceCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<UnitOpeningBalanceReadDto> Consume(
        SetUnitOpeningBalanceCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var unit = await session.Query<RealEstateUnit>()
            .FirstOrDefaultAsync(x => x.Id == command.UnitId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (unit == null)
            throw new NotFoundException("Unità immobiliare non trovata.");

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.FiscalYearId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null)
            throw new NotFoundException("Esercizio non trovato.");

        // Blocca se l'esercizio è già chiuso
        if (fiscalYear.Status?.Id == FiscalYearStatus.Closed || fiscalYear.Status?.Id == FiscalYearStatus.Locked)
            throw new ValidatorException("Non è possibile modificare il bilancio: l'esercizio è già chiuso.");

        // Blocca se esiste un esercizio precedente (OpeningBalance propagato automaticamente)
        if (fiscalYear.PreviousFiscalYear != null)
            throw new ValidatorException(
                "Il bilancio di apertura non è modificabile: viene propagato automaticamente dal saldo di chiusura dell'esercizio precedente.");

        // Upsert
        var record = await session.Query<Models.UnitOpeningBalance>()
            .FirstOrDefaultAsync(x => x.Unit.Id == command.UnitId
                                   && x.FiscalYear.Id == command.Dto.FiscalYearId
                                   && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (record == null)
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
        record.OpeningBalance  = command.Dto.OpeningBalance;
        // I campi di movimento non vengono toccati qui: appartengono all'esercizio in corso
        // e vengono calcolati automaticamente (rate dal preventivo, conguaglio dal consuntivo).
        record.TotalMovements  = 0;
        record.ClosingBalance  = 0;
        record.Notes           = command.Dto.Notes;

        await session.SaveOrUpdateAsync(record, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return new UnitOpeningBalanceReadDto
        {
            Id              = record.Id,
            UnitId          = command.UnitId,
            UnitName        = unit.FormatUnitName(),
            FiscalYearId    = fiscalYear.Id,
            FiscalYearCode  = fiscalYear.Code,
            OpeningBalance  = record.OpeningBalance,
            RateAddebitate  = record.RateAddebitate,
            RateIncassate   = record.RateIncassate,
            QuotaConsuntiva = record.QuotaConsuntiva,
            SaldoConguaglio = record.SaldoConguaglio,
            TotalMovements  = 0,
            ClosingBalance  = 0,
            Notes           = record.Notes,
            IsEditable      = true,
            IsClosed        = false,
        };
    }
}
