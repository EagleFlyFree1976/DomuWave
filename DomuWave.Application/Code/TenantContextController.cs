using CPQ.Core.Settings;
using DomuWave.Application.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Application.Code;

[ServiceFilter(typeof(TenantHeaderFilter))]
public class TenantContextController : PrivateControllerBase
{
    

    public TenantContextController(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
    {
    }
}