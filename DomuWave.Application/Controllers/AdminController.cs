using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Controllers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
 


namespace DomuWave.Microservice.Controllers
{
    /// <summary>
    ///  Gestione piattaforma
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    [Route("api/[controller]")]
    public class AdminController : OxCoreTokenAuthorizeControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly ISessionFactoryProvider _sessionFactory;


     

        protected readonly IBusControl _busControl;
        protected readonly ICoreAuthorizationManager _authorizationManager;

        public AdminController(ILogger<AdminController> logger, IOptionsMonitor<OxCoreSettings> configuration, IBusControl busControl, ISessionFactoryProvider sessionFactory, ICoreAuthorizationManager authorizationManager) : base(logger, configuration)
        {
            _busControl = busControl;
            _sessionFactory = sessionFactory;
            _authorizationManager = authorizationManager;
        }



        [HttpGet("decrypt")]
        public string GetDec(string v, CancellationToken cancellationToken)
        {

            _logger.LogInformation($"decrypt- {v}");
            return v.DecryptString();


        }
        [HttpGet("crypt")]
        public string GetCrypt(string v, CancellationToken cancellationToken)
        {


            return v.EncryptString();


        }

        [HttpGet("reset")]
        public async Task<string> ResetAuth(CancellationToken cancellationToken)
        {
            _logger.LogInformation("reset auth ");

            await _authorizationManager.ClearCacheAsync(cancellationToken).ConfigureAwait(false);

        
            return "DONE";


        }

        [HttpGet("throw")]
        public Task<IActionResult> GetThrowException(CancellationToken cancellationToken)
        {


            throw new Exception("TEST");

        
        }
    }
}