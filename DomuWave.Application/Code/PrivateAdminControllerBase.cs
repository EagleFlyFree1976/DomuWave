using DomuWave.Application.Filters;
using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Application.Code;

//[ServiceFilter(typeof(SystemBookHeaderFilter))]
public class PrivateAdminControllerBase : OxCoreTokenAuthorizeControllerBase
{

    protected Guid? TenantId
    {
        get

        {

            if (Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValue))
            {
                return Guid.Parse(tenantIdValue);
            }

            return null;
        }
    }
    public PrivateAdminControllerBase(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
    {
    }
}