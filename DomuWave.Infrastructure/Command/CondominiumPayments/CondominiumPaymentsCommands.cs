using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumPayments;

/// <summary>
/// Avvia (o riprende) l'onboarding Stripe Connect per un condominio.
/// Crea il connected account se assente e restituisce l'URL di onboarding Stripe.
/// Riservato all'amministratore.
/// </summary>
public class StartStripeOnboardingCommand : BaseCommand, IQuery<string>
{
    public int CondominiumId { get; set; }

    public StartStripeOnboardingCommand() { }
    public StartStripeOnboardingCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

/// <summary>
/// Interroga Stripe sullo stato del connected account del condominio e aggiorna
/// <c>StripeOnboardingComplete</c>. Restituisce true se il condominio può incassare online.
/// Riservato all'amministratore.
/// </summary>
public class RefreshStripeAccountStatusCommand : BaseCommand, IQuery<bool>
{
    public int CondominiumId { get; set; }

    public RefreshStripeAccountStatusCommand() { }
    public RefreshStripeAccountStatusCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

/// <summary>
/// Avvia il pagamento online di una singola quota (CondominiumFee) da parte del condòmino.
/// Restituisce l'URL del Checkout Stripe a cui reindirizzare l'utente.
/// </summary>
public class InitiateFeePaymentCommand : BaseCommand, IQuery<string>
{
    public long FeeId { get; set; }

    public InitiateFeePaymentCommand() { }
    public InitiateFeePaymentCommand(int currentUserId, long feeId) : base(currentUserId)
        => FeeId = feeId;
}

/// <summary>
/// Conferma un pagamento Stripe ricevuto via webhook: riconcilia la quota e crea il Payment.
/// Idempotente rispetto a <see cref="ProviderTransactionId"/> (il PaymentIntent id).
/// Non ha un CurrentUser umano: l'azione è di sistema (innescata dal webhook).
/// </summary>
public class ConfirmStripePaymentCommand : BaseCommand, IQuery<bool>
{
    public long FeeId { get; set; }
    public decimal Amount { get; set; }
    public string ProviderTransactionId { get; set; }
    public long PayerUserId { get; set; }

    public ConfirmStripePaymentCommand() { }
    public ConfirmStripePaymentCommand(long feeId, decimal amount, string providerTransactionId, long payerUserId)
    {
        FeeId = feeId;
        Amount = amount;
        ProviderTransactionId = providerTransactionId;
        PayerUserId = payerUserId;
    }
}
