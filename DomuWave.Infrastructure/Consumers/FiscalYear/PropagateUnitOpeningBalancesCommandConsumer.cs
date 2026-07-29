using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;
using Models = DomuWave.Services.Models;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Ri-propaga i saldi iniziali per unità dall'esercizio precedente a quello indicato.
/// Utile quando l'esercizio è già stato creato ma i saldi non sono stati copiati
/// (es. l'esercizio precedente non era ancora chiuso al momento della creazione).
/// Restituisce il numero di record creati o aggiornati.
/// </summary>
public class PropagateUnitOpeningBalancesCommandConsumer
    : InMemoryConsumerBase<PropagateUnitOpeningBalancesCommand, int>
{
    private readonly IUserService _userService;

    public PropagateUnitOpeningBalancesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<int> Consume(
        PropagateUnitOpeningBalancesCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var fiscalYear = await session.Query<FiscalYear>()
            .FirstOrDefaultAsync(f => f.Id == command.FiscalYearId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (fiscalYear == null)
            throw new NotFoundException("Esercizio fiscale non trovato.");

        if (fiscalYear.PreviousFiscalYear == null)
            throw new ValidatorException(
                "Questo è il primo esercizio del condominio: i saldi iniziali devono essere inseriti manualmente.");

        var previousFiscalYearId = fiscalYear.PreviousFiscalYear.Id;

        var previousBalances = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == previousFiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!previousBalances.Any())
            throw new ValidatorException(
                "L'esercizio precedente non ha saldi per unità. Chiudi prima l'esercizio precedente.");

        // Carica i record esistenti per aggiornare o creare
        var existingRecords = await session.Query<Models.UnitOpeningBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYear.Id && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingByUnitId = existingRecords.ToDictionary(r => r.Unit.Id);

        var user = currentUser as CPQ.Core.Memberships.IUser;
        var count = 0;

        foreach (var prev in previousBalances)
        {
            if (existingByUnitId.TryGetValue(prev.Unit.Id, out var existing))
            {
                // Aggiorna il saldo di apertura mantenendo i movimenti già registrati
                existing.OpeningBalance = prev.ClosingBalance;
                existing.Trace(currentUser);
                await session.SaveOrUpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var newBalance = new Models.UnitOpeningBalance
                {
                    Unit            = prev.Unit,
                    FiscalYear      = fiscalYear,
                    Tenant          = fiscalYear.Tenant,
                    OpeningBalance  = prev.ClosingBalance,
                    RateAddebitate  = 0m,
                    RateIncassate   = 0m,
                    QuotaConsuntiva = 0m,
                    SaldoConguaglio = 0m,
                    TotalMovements  = 0m,
                    ClosingBalance  = 0m,
                };
                if (user != null) newBalance.Trace(user);
                await session.SaveAsync(newBalance, cancellationToken).ConfigureAwait(false);
            }
            count++;
        }

        // ── Propaga anche i saldi di gruppo (mai spalmati sulle unità componenti) ──
        var previousGroupBalances = await session.Query<Models.BillingGroupOpeningBalance>()
            .Where(b => b.FiscalYear.Id == previousFiscalYearId && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingGroupRecords = await session.Query<Models.BillingGroupOpeningBalance>()
            .Where(b => b.FiscalYear.Id == fiscalYear.Id && !b.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingByGroupId = existingGroupRecords.ToDictionary(r => r.BillingGroup.Id);

        foreach (var prev in previousGroupBalances)
        {
            if (existingByGroupId.TryGetValue(prev.BillingGroup.Id, out var existing))
            {
                existing.OpeningBalance = prev.ClosingBalance;
                existing.Trace(currentUser);
                await session.SaveOrUpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var newGroupBalance = new Models.BillingGroupOpeningBalance
                {
                    BillingGroup    = prev.BillingGroup,
                    FiscalYear      = fiscalYear,
                    Tenant          = fiscalYear.Tenant,
                    OpeningBalance  = prev.ClosingBalance,
                    RateAddebitate  = 0m,
                    RateIncassate   = 0m,
                    QuotaConsuntiva = 0m,
                    SaldoConguaglio = 0m,
                    TotalMovements  = 0m,
                    ClosingBalance  = 0m,
                };
                if (user != null) newGroupBalance.Trace(user);
                await session.SaveAsync(newGroupBalance, cancellationToken).ConfigureAwait(false);
            }
            count++;
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return count;
    }
}
