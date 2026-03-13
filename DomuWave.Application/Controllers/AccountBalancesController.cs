using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.AccountBalance;
using DomuWave.Services.Dto.Contabilita.AccountBalance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/account-balances")]
[Produces("application/json")]
public class AccountBalancesController(
    ILogger<AccountBalancesController> logger,
    IOptionsMonitor<OxCoreSettings>    configuration,
    IMediator                          mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-fiscal-year/{fiscalYearId:int}")]
    [ProducesResponseType(typeof(IList<AccountBalanceReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByFiscalYear(int fiscalYearId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetAccountBalancesByFiscalYearCommand(CurrentUser.Id, fiscalYearId), ct);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AccountBalanceReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOpening(
        int id, [FromBody] UpdateAccountBalanceOpeningDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new UpdateAccountBalanceOpeningCommand(CurrentUser.Id, id, dto), ct);
        return Ok(result);
    }
}
