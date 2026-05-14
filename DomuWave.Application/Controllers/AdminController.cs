using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Controllers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using CPQ.Core.Extensions;
using DomuWave.Services.Command.Admin;
using DomuWave.Services.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;



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
        private readonly IMediator _mediator;

        public AdminController(
            ILogger<AdminController> logger,
            IOptionsMonitor<OxCoreSettings> configuration,

            ISessionFactoryProvider sessionFactory,
            ICoreAuthorizationManager authorizationManager,
            IBackgroundJobClient backgroundJobClient,
            IMediator mediator) : base(logger, configuration)
        {

            _sessionFactory        = sessionFactory;
            _authorizationManager  = authorizationManager;
            _backgroundJobClient   = backgroundJobClient;
            _mediator              = mediator;
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

        [HttpPost("hilo/align")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AlignHilo(CancellationToken ct)
        {
            var result = await _mediator.GetResponse(new AlignHiloCommand(CurrentUser.Id), ct);
            return Ok(result);
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