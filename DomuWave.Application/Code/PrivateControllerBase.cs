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
                if (HttpContext.Items.ContainsKey("TenantId"))
                {
                    return (Guid)HttpContext.Items["TenantId"];
                }
                return null;
            }
        }

        public PrivateControllerBase(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
        {
        }
    }
}
