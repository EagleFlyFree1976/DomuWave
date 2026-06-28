using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Command.CondominiumPayments;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Avvia il Checkout Stripe per il pagamento di una singola quota da parte del condòmino.
/// </summary>
public class InitiateFeePaymentCommandConsumer
    : InMemoryConsumerBase<InitiateFeePaymentCommand, string>
{
    private readonly IUserService   _userService;
    private readonly IStripeService _stripeService;

    public InitiateFeePaymentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IStripeService          stripeService) : base(sessionFactoryProvider)
    {
        _userService   = userService;
        _stripeService = stripeService;
    }

    protected override async Task<string> Consume(
        InitiateFeePaymentCommand command,
        IMediationContext         mediationContext,
        CancellationToken         cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var fee = await session.Query<CondominiumFee>()
            .FirstOrDefaultAsync(f => f.Id == command.FeeId && !f.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Quota non trovata.");

        // Autorizzazione: un condòmino può pagare solo le proprie quote.
        // (long)currentUser.Id è l'auth user id, lo stesso valore salvato in CondominiumFee.UserId.
        if (fee.UserId != (long)currentUser.Id)
            throw new ValidatorException("Non è possibile pagare una quota che non ti appartiene.");

        if (fee.PaymentStatus == "Paid" || fee.Balance <= 0)
            throw new ValidatorException("La quota è già stata saldata.");

        var condominium = fee.Installment?.Condominium
            ?? throw new NotFoundException("Condominio della quota non trovato.");

        if (!condominium.StripeOnboardingComplete || string.IsNullOrWhiteSpace(condominium.StripeConnectedAccountId))
            throw new ValidatorException("I pagamenti online non sono ancora attivi per questo condominio.");

        var unit = fee.Unit;
        var description = $"Quota condominiale {condominium.Name}"
            + (unit != null ? $" - unità {unit.InternalNumber}" : string.Empty);

        var metadata = new Dictionary<string, string>
        {
            ["feeId"]         = fee.Id.ToString(),
            ["condominiumId"] = condominium.Id.ToString(),
            ["unitId"]        = (unit?.Id ?? 0).ToString(),
            ["userId"]        = fee.UserId.ToString(),
        };

        return await _stripeService.CreateFeeCheckoutSessionAsync(
            condominium.StripeConnectedAccountId,
            fee.Balance,
            description,
            metadata,
            cancellationToken).ConfigureAwait(false);
    }
}
