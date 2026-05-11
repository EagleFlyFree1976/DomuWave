using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Controllers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Services.Jobs;
using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Http;
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


     
         
        protected readonly ICoreAuthorizationManager _authorizationManager;
        protected readonly IBackgroundJobClient _backgroundJobClient;

        public AdminController(
            ILogger<AdminController> logger,
            IOptionsMonitor<OxCoreSettings> configuration,
       
            ISessionFactoryProvider sessionFactory,
            ICoreAuthorizationManager authorizationManager,
            IBackgroundJobClient backgroundJobClient) : base(logger, configuration)
        {
    
            _sessionFactory        = sessionFactory;
            _authorizationManager  = authorizationManager;
            _backgroundJobClient   = backgroundJobClient;
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

        [HttpPost("jobs/truncate-old-log")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult EnqueueTruncateOldLog()
        {

            var jobId = _backgroundJobClient.Enqueue<TruncateOldLogJob>(j => j.RunAsync(CancellationToken.None));
            _logger.LogInformation("[Admin] TruncateOldLogJob accodato con id {JobId}", jobId);
            return Accepted(new { jobId, message = "Job sp_domus_truncateoldlog accodato" });
        }
    }
}