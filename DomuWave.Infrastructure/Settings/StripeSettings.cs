namespace DomuWave.Services.Settings;

/// <summary>
/// Impostazioni Stripe della piattaforma DomuWave (non del singolo condominio).
/// Le chiavi appartengono all'account Stripe della piattaforma; i singoli condomìni
/// sono "connected account" collegati tramite Stripe Connect (Express).
/// </summary>
public class StripeSettings
{
    /// <summary>Secret key della piattaforma (sk_test_... / sk_live_...).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Publishable key della piattaforma (pk_test_... / pk_live_...).</summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>Webhook signing secret (whsec_...) per verificare la firma degli eventi.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>URL a cui Stripe riporta l'amministratore al termine dell'onboarding Connect.</summary>
    public string ConnectReturnUrl { get; set; } = string.Empty;

    /// <summary>URL a cui Stripe riporta l'amministratore se il link di onboarding è scaduto.</summary>
    public string ConnectRefreshUrl { get; set; } = string.Empty;

    /// <summary>URL di ritorno dopo un pagamento Checkout andato a buon fine.</summary>
    public string CheckoutSuccessUrl { get; set; } = string.Empty;

    /// <summary>URL di ritorno se il condòmino annulla il pagamento Checkout.</summary>
    public string CheckoutCancelUrl { get; set; } = string.Empty;
}
