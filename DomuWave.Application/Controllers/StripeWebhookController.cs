using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Services.Clients;
using DomuWave.Services.Command.CondominiumPayments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using Stripe;
using Stripe.Checkout;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Endpoint pubblico per gli eventi webhook di Stripe (nessun token JWT, nessun X-Tenant-Id).
/// Il tenant/condominio viene risolto dai metadata dell'evento, non dagli header.
/// </summary>
[Route("api/stripe")]
[NoAccessTokenRequired]
public class StripeWebhookController(
    ILogger<StripeWebhookController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IStripeService stripeService,
    IMediator mediator)
    : OxCoreControllerBase(logger, configuration)
{
    private readonly IStripeService _stripeService = stripeService;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<StripeWebhookController> _logger = logger;

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync(ct).ConfigureAwait(false);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        Event stripeEvent;
        try
        {
            stripeEvent = _stripeService.ConstructWebhookEvent(payload, signature);
        }
        catch (StripeException ex)
        {
            // Firma non valida → 400, Stripe non ritenterà come fosse un errore server.
            _logger.LogWarning(ex, "Webhook Stripe: firma non valida.");
            return BadRequest();
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            if (stripeEvent.Data.Object is not Session session)
                return Ok();

            if (session.Metadata == null
                || !session.Metadata.TryGetValue("feeId", out var feeIdRaw)
                || !long.TryParse(feeIdRaw, out var feeId))
            {
                _logger.LogWarning("Webhook Stripe: metadata feeId mancante o non valido.");
                return Ok();
            }

            session.Metadata.TryGetValue("userId", out var userIdRaw);
            long.TryParse(userIdRaw, out var payerUserId);

            // amount_total è in centesimi.
            var amount = (session.AmountTotal ?? 0L) / 100m;

            // PaymentIntent id = riferimento univoco per l'idempotenza.
            var providerTransactionId = session.PaymentIntentId ?? session.Id;

            await _mediator.GetResponse(
                new ConfirmStripePaymentCommand(feeId, amount, providerTransactionId, payerUserId), ct)
                .ConfigureAwait(false);
        }

        // Per gli eventi non gestiti rispondiamo comunque 200 così Stripe non ritenta.
        return Ok();
    }
}
