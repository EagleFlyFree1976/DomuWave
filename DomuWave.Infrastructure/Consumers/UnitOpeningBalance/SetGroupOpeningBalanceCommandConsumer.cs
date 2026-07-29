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

        if (fiscalYear.PreviousFiscalYear != null)
            throw new ValidatorException(
                "Il bilancio di apertura non è modificabile: viene propagato automaticamente dal saldo di chiusura dell'esercizio precedente.");

        // Upsert diretto sul gruppo — il saldo NON viene ripartito sulle unità componenti.
        var record = await session.Query<Models.BillingGroupOpeningBalance>()
            .FirstOrDefaultAsync(b => b.BillingGroup.Id == command.BillingGroupId
                                   && b.FiscalYear.Id == command.Dto.FiscalYearId
                                   && !b.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (record == null)
        {
            record = new Models.BillingGroupOpeningBalance
            {
                BillingGroup = group,
                FiscalYear   = fiscalYear,
                Tenant       = group.Tenant,
                IsDeleted    = false,
            };
        }

        record.Trace(currentUser);
        record.OpeningBalance = command.Dto.OpeningBalance;
        record.Notes          = command.Dto.Notes;

        await session.SaveOrUpdateAsync(record, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
