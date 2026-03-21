using Auth.Services.Command;
using Auth.Services.Extensions;
using Auth.Services.Models.Dto;
using Auth.Services.Orchestators;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Microservice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

[Route("api/auth")]
[NoAccessTokenRequired]
public class AuthPublicController(
    ILogger<AuthPublicController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    AuthOrchestator authOrchestator,
    IMemoryCache cache)
    : OxCoreControllerBase(logger, configuration)
{
    protected readonly AuthOrchestator _authOrchestator = authOrchestator;
    private readonly IMemoryCache _cache = cache;
    protected readonly IUserService _userService = userService;

    [HttpPost("login")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BaseDto))]
    public async Task<IActionResult> GetByLogin([FromBody] UserLogin logininfo, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByEmailAsync(logininfo.Email, cancellationToken).ConfigureAwait(false);

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
    public async Task<IActionResult> ResetPasswordByEmail(string email, CancellationToken cancellationToken)
    {
        await _authOrchestator.GeneratePasswordResetAsync(email, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("confirm-reset-password")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authOrchestator.ConfirmPasswordResetAsync(request.Token, request.NewPassword, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
