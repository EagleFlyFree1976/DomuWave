using DomuWave.Services.Models;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace DomuWave.Services.Clients;

/// <summary>
/// Implementazione di <see cref="IStripeService"/> basata su Stripe.net.
/// Le chiamate verso i connected account usano <see cref="RequestOptions.StripeAccount"/>
/// (direct charge): l'incasso finisce direttamente sul conto del condominio.
/// </summary>
public class StripeService : IStripeService
{
    private readonly StripeSettings _settings;

    public StripeService(IOptions<StripeSettings> options)
    {
        _settings = options.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<string> CreateConnectedAccountAsync(Condominium condominium, CancellationToken cancellationToken)
    {
        var options = new AccountCreateOptions
        {
            Type = "express",
            Country = "IT",
            Email = string.IsNullOrWhiteSpace(condominium.Email) ? condominium.AdministratorEmail : condominium.Email,
            BusinessType = "non_profit",
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
            },
            BusinessProfile = new AccountBusinessProfileOptions
            {
                Name = condominium.Name,
                ProductDescription = "Riscossione quote condominiali",
            },
            Metadata = new Dictionary<string, string>
            {
                ["condominiumId"] = condominium.Id.ToString(),
            },
        };

        var service = new AccountService();
        var account = await service.CreateAsync(options, cancellationToken: cancellationToken).ConfigureAwait(false);
        return account.Id;
    }

    public async Task<string> CreateAccountLinkAsync(string connectedAccountId, CancellationToken cancellationToken)
    {
        var options = new AccountLinkCreateOptions
        {
            Account = connectedAccountId,
            RefreshUrl = _settings.ConnectRefreshUrl,
            ReturnUrl = _settings.ConnectReturnUrl,
            Type = "account_onboarding",
        };

        var service = new AccountLinkService();
        var link = await service.CreateAsync(options, cancellationToken: cancellationToken).ConfigureAwait(false);
        return link.Url;
    }

    public async Task<bool> IsAccountOnboardedAsync(string connectedAccountId, CancellationToken cancellationToken)
    {
        var service = new AccountService();
        var account = await service.GetAsync(connectedAccountId, cancellationToken: cancellationToken).ConfigureAwait(false);
        return account.ChargesEnabled && account.DetailsSubmitted;
    }

    public async Task<string> CreateFeeCheckoutSessionAsync(
        string connectedAccountId,
        decimal amount,
        string description,
        IDictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        // Stripe ragiona in centesimi (unità minima della valuta).
        var amountInCents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = _settings.CheckoutSuccessUrl,
            CancelUrl = _settings.CheckoutCancelUrl,
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = description,
                        },
                    },
                },
            },
            // I metadata vengono propagati sia sulla session sia sul payment_intent,
            // così il webhook può riconciliare la quota.
            Metadata = new Dictionary<string, string>(metadata),
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>(metadata),
            },
        };

        // Direct charge: la session viene creata SUL connected account del condominio.
        var requestOptions = new RequestOptions { StripeAccount = connectedAccountId };

        var service = new SessionService();
        var session = await service.CreateAsync(options, requestOptions, cancellationToken).ConfigureAwait(false);
        return session.Url;
    }

    public Event ConstructWebhookEvent(string payload, string signatureHeader)
    {
        // Lancia StripeException se la firma non è valida.
        return EventUtility.ConstructEvent(payload, signatureHeader, _settings.WebhookSecret);
    }
}
