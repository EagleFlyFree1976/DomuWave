using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

public class AuthorizationBaseController : OxCoreTokenAuthorizeControllerBase
{
    public AuthorizationBaseController(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration)
        : base(logger, configuration)
    {
    }
}
