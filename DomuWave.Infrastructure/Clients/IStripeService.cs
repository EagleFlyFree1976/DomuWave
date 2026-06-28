using DomuWave.Services.Models;
using Stripe;

namespace DomuWave.Services.Clients;

/// <summary>
/// Wrapper sulle API Stripe (Connect Express) usate da DomuWave per i pagamenti online
/// delle quote condominiali. Ogni condominio è un connected account; la piattaforma
/// DomuWave non transita i fondi (direct charge sul connected account).
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Crea un connected account Stripe Express per il condominio e ne restituisce l'id.
    /// </summary>
    Task<string> CreateConnectedAccountAsync(Condominium condominium, CancellationToken cancellationToken);

    /// <summary>
    /// Genera un Account Link per l'onboarding (KYC + IBAN) del connected account.
    /// Restituisce l'URL a cui reindirizzare l'amministratore.
    /// </summary>
    Task<string> CreateAccountLinkAsync(string connectedAccountId, CancellationToken cancellationToken);

    /// <summary>
    /// Interroga Stripe sullo stato del connected account.
    /// Restituisce true se il condominio può incassare (charges_enabled &amp;&amp; details_submitted).
    /// </summary>
    Task<bool> IsAccountOnboardedAsync(string connectedAccountId, CancellationToken cancellationToken);

    /// <summary>
    /// Crea una Checkout Session (direct charge sul connected account del condominio) per pagare
    /// una singola quota. I <paramref name="metadata"/> trasportano feeId/condominiumId/unitId/userId
    /// che il webhook userà per riconciliare. Restituisce l'URL del Checkout.
    /// </summary>
    Task<string> CreateFeeCheckoutSessionAsync(
        string connectedAccountId,
        decimal amount,
        string description,
        IDictionary<string, string> metadata,
        CancellationToken cancellationToken);

    /// <summary>
    /// Verifica la firma del webhook e deserializza l'evento Stripe.
    /// Lancia <see cref="StripeException"/> se la firma non è valida.
    /// </summary>
    Event ConstructWebhookEvent(string payload, string signatureHeader);
}
