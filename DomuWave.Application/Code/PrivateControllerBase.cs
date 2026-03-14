using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Application.Code
{
 //   [ServiceFilter(typeof(TenantHeaderFilter))]
    public class PrivateControllerBase : OxCoreTokenAuthorizeControllerBase
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

        public PrivateControllerBase(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
        {
        }
    }
}
