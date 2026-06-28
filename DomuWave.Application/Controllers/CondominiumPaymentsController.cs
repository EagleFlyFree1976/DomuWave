using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.CondominiumPayments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Pagamenti online delle quote condominiali (Stripe Connect).
/// Onboarding lato amministratore + avvio pagamento lato condòmino.
/// </summary>
[Route("api/condominium-payments")]
[Produces("application/json")]
public class CondominiumPaymentsController(
    ILogger<CondominiumPaymentsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    /// <summary>Avvia/riprende l'onboarding Stripe per il condominio. Restituisce l'URL di onboarding.</summary>
    [HttpPost("stripe/onboarding/{condominiumId:int}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> StartOnboarding(int condominiumId, CancellationToken ct)
    {
        var url = await _mediator.GetResponse(
            new StartStripeOnboardingCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(new { url });
    }

    /// <summary>Aggiorna lo stato di onboarding Stripe del condominio interrogando Stripe.</summary>
    [HttpPost("stripe/refresh/{condominiumId:int}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RefreshStatus(int condominiumId, CancellationToken ct)
    {
        var complete = await _mediator.GetResponse(
            new RefreshStripeAccountStatusCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(new { complete });
    }

    /// <summary>Avvia il pagamento online di una quota da parte del condòmino. Restituisce l'URL del Checkout.</summary>
    [HttpPost("stripe/initiate/{feeId:long}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> InitiatePayment(long feeId, CancellationToken ct)
    {
        var url = await _mediator.GetResponse(
            new InitiateFeePaymentCommand(CurrentUser.Id, feeId), ct);
        return Ok(new { url });
    }
}
