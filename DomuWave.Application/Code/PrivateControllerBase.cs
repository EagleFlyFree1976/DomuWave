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
        protected long TenantId
        {
            get
            {
                return (long)HttpContext.Items["TenantId"];
            }
        }

        public PrivateControllerBase(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
        {
        }
    }
}
