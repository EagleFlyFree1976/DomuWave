using DomuWave.Application.Filters;
using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Application.Code
{
 //   [ServiceFilter(typeof(TenantHeaderFilter))]
    public class PrivateControllerBase : OxCoreTokenAuthorizeControllerBase
    {
        protected long? TenantId
        {
            get

            {
                if (HttpContext.Items.ContainsKey("TenantId"))
                {
                    return (long)HttpContext.Items["TenantId"];
                }
                return null;
            }
        }

        public PrivateControllerBase(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
        {
        }
    }
}
