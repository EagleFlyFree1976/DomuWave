using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.BillingGroup;
using DomuWave.Services.Command.UnitOpeningBalance;
using DomuWave.Services.Dto.BillingGroup;
using DomuWave.Services.Dto.UnitOpeningBalance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/billing-groups")]
[Produces("application/json")]
public class BillingGroupsController(
    ILogger<BillingGroupsController> logger,
    IOptionsMonitor<OxCoreSettings>  configuration,
    IMediator                        mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<BillingGroupReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetBillingGroupsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BillingGroupReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateBillingGroupDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateBillingGroupCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BillingGroupReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBillingGroupDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateBillingGroupCommand(CurrentUser.Id, id, dto), ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.GetResponse(new DeleteBillingGroupCommand(CurrentUser.Id, id), ct);
        return NoContent();
    }

    [HttpGet("suggest/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(SuggestBillingGroupsResultDto), 200)]
    public async Task<IActionResult> Suggest(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new SuggestBillingGroupsCommand(CurrentUser.Id, condominiumId), ct));

    [HttpPost("apply-suggestions/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<BillingGroupReadDto>), 201)]
    public async Task<IActionResult> ApplySuggestions(int condominiumId, [FromBody] IList<BillingGroupSuggestionDto> suggestions, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new ApplySuggestedBillingGroupsCommand(CurrentUser.Id, condominiumId, suggestions), ct);
        return StatusCode(201, result);
    }

    [HttpGet("{id:int}/fiscal-year/{fiscalYearId:int}/notice/pdf")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetGroupNoticePdf(int id, int fiscalYearId, CancellationToken ct)
    {
        var bytes = await _mediator.GetResponse(
            new GetBillingGroupPaymentNoticeCommand(CurrentUser.Id, id, fiscalYearId), ct);
        return File(bytes, "application/pdf", $"avviso-gruppo-{id}-esercizio-{fiscalYearId}.pdf");
    }

    [HttpGet("{id:int}/installment/{installmentId:int}/notice/pdf")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetGroupInstallmentNoticePdf(int id, int installmentId, CancellationToken ct)
    {
        var bytes = await _mediator.GetResponse(
            new GetBillingGroupInstallmentNoticeCommand(CurrentUser.Id, id, installmentId), ct);
        return File(bytes, "application/pdf", $"avviso-gruppo-{id}-rata-{installmentId}.pdf");
    }

    [HttpGet("{id:int}/opening-balance")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UnitOpeningBalanceReadDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOpeningBalance(int id, [FromQuery] int fiscalYearId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetGroupOpeningBalanceCommand(CurrentUser.Id, id, fiscalYearId), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:int}/opening-balance")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.BillingGroups, Modules.DomuWaveModule)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SetOpeningBalance(int id, [FromBody] SetGroupOpeningBalanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mediator.GetResponse(new SetGroupOpeningBalanceCommand(CurrentUser.Id, id, dto), ct);
        return NoContent();
    }
}
