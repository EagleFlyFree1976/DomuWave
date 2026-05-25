using Auth.Services.Command;
using Auth.Services.Extensions;
using Auth.Services.Models.Dto;
using Auth.Services.Orchestators;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Extensions;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Microservice.Models;
using DomuWave.Services.Command.Auth;
using DomuWave.Services.Dto.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/auth")]
[NoAccessTokenRequired]
public class AuthPublicController(
    ILogger<AuthPublicController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    AuthOrchestator authOrchestator,
    IMediator mediator,
    IMemoryCache cache)
    : OxCoreControllerBase(logger, configuration)
{
    protected readonly AuthOrchestator _authOrchestator = authOrchestator;
    private readonly IMemoryCache _cache = cache;
    protected readonly IUserService _userService = userService;
    private readonly IMediator _mediator = mediator;

    [HttpPost("login")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BaseDto))]
    public async Task<IActionResult> GetByLogin([FromBody] UserLogin logininfo, CancellationToken cancellationToken)
    {
        Auth.Services.Models.User user = (Auth.Services.Models.User)await _userService.GetByEmailAsync(logininfo.Email, cancellationToken).ConfigureAwait(false);

        if (user == null || user.Password != logininfo.Password.EncryptString())
            return NotFound();

        return Ok(user.ToDto());
    }

    [HttpPost("register")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BaseDto))]
    public async Task<IActionResult> PostRegister([FromBody] RegisterUser registerInfo, CancellationToken cancellationToken)
    {
        var user = await _authOrchestator.CreateUser(new CreateUser
        {
            Email      = registerInfo.Email,
            Name       = registerInfo.Name,
            Password   = registerInfo.Password,
            SurName    = registerInfo.SurName,
            RoleCode   = registerInfo.RoleCode,
            ModuleCode = registerInfo.ModuleCode,
        }, cancellationToken).ConfigureAwait(false);

        return Ok(user.ToDto());
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPasswordByEmail([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authOrchestator.GeneratePasswordResetAsync(request.Email, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    public record ResetPasswordRequest(string Email);

    [HttpPost("confirm-reset-password")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authOrchestator.ConfirmPasswordResetAsync(request.Token, request.NewPassword, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Verifica se una email appartiene a un utente Condomino già registrato.
    /// </summary>
    [HttpPost("check-email")]
    [ProducesResponseType(typeof(CheckEmailResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmail([FromBody] CheckEmailDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.GetResponse(new CheckEmailCommand(dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Avvia la registrazione pubblica: valida l'email, salva i dati in staging e restituisce il GUID
    /// da usare come Id del tenant al momento della conferma (dopo il pagamento).
    /// </summary>
    [HttpPost("self-register")]
    [ProducesResponseType(typeof(SelfRegisterResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SelfRegister([FromBody] SelfRegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.GetResponse(new SelfRegisterCommand(dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Conferma la registrazione dopo il pagamento: crea l'utente (o promuove il Condomino ad Admin),
    /// crea il tenant con l'Id uguale al RegistrationId, e restituisce il token di sessione.
    /// </summary>
    [HttpPost("confirm-registration")]
    [ProducesResponseType(typeof(ConfirmRegistrationResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.GetResponse(new ConfirmRegistrationCommand(dto), cancellationToken);
        return Ok(result);
    }
}
