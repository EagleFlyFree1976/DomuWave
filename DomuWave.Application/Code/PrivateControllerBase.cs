using DomuWave.Application.Filters;
using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Application.Code
{
    [ServiceFilter(typeof(BookHeaderFilter))]
    public class PrivateControllerBase : OxCoreTokenAuthorizeControllerBase
    {
        protected long BookId
        {
            get
            {
                return (long)HttpContext.Items["BookId"];
            }
        }

        public PrivateControllerBase(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
        {
        }
    }
}
