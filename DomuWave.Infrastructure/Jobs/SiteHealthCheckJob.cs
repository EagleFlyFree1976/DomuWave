using CPQ.Core.Startups;
using DomuWave.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DomuWave.Services.Jobs;

public class SiteHealthCheckJob : IRecurrenceAsyncJob
{
    private readonly IHttpClientFactory         _httpClientFactory;
    private readonly IEmailService              _emailService;
    private readonly IConfiguration             _configuration;
    private readonly ILogger<SiteHealthCheckJob> _logger;

    public SiteHealthCheckJob(
        IHttpClientFactory          httpClientFactory,
        IEmailService               emailService,
        IConfiguration              configuration,
        ILogger<SiteHealthCheckJob> logger)
    {
        _httpClientFactory = httpClientFactory;
        _emailService      = emailService;
        _configuration     = configuration;
        _logger            = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var url           = _configuration["HealthCheck:Url"]          ?? string.Empty;
        var notifyEmail   = _configuration["HealthCheck:NotifyEmail"]   ?? string.Empty;
        var timeoutSec    = int.TryParse(_configuration["HealthCheck:TimeoutSeconds"], out var t) ? t : 15;
        var maxStatusCode = int.TryParse(_configuration["HealthCheck:MaxStatusCode"],  out var s) ? s : 399;

        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("[SiteHealthCheckJob] HealthCheck:Url non configurato — job saltato.");
            return;
        }

        _logger.LogInformation("[SiteHealthCheckJob] Controllo {Url}", url);

        string? errorReason = null;
        int?    statusCode  = null;

        try
        {
            var client = _httpClientFactory.CreateClient("healthcheck");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSec));

            var response = await client.GetAsync(url, cts.Token).ConfigureAwait(false);
            statusCode = (int)response.StatusCode;

            if (statusCode > maxStatusCode)
                errorReason = $"HTTP {statusCode}";
        }
        catch (TaskCanceledException)
        {
            errorReason = $"Timeout dopo {timeoutSec}s";
        }
        catch (Exception ex)
        {
            errorReason = ex.Message;
        }

        if (errorReason == null)
        {
            _logger.LogInformation("[SiteHealthCheckJob] {Url} OK (HTTP {StatusCode})", url, statusCode);
            return;
        }

        _logger.LogError("[SiteHealthCheckJob] {Url} NON RAGGIUNGIBILE — {Reason}", url, errorReason);

        if (string.IsNullOrWhiteSpace(notifyEmail))
        {
            _logger.LogWarning("[SiteHealthCheckJob] HealthCheck:NotifyEmail non configurato — notifica email saltata.");
            return;
        }

        var smtpConfig = BuildSmtpConfig();
        if (smtpConfig == null)
        {
            _logger.LogWarning("[SiteHealthCheckJob] SMTP non configurato — notifica email saltata.");
            return;
        }

        try
        {
            var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var message = new EmailMessage(
                To:       notifyEmail,
                ToName:   "DomuWave Admin",
                Subject:  $"[DomuWave] ⚠ Sito non raggiungibile — {now}",
                BodyHtml: $"""
                    <p>Il monitoraggio automatico ha rilevato un problema.</p>
                    <table style="border-collapse:collapse;font-family:monospace">
                      <tr><td style="padding:4px 12px 4px 0"><b>URL</b></td><td>{url}</td></tr>
                      <tr><td style="padding:4px 12px 4px 0"><b>Errore</b></td><td>{errorReason}</td></tr>
                      <tr><td style="padding:4px 12px 4px 0"><b>Rilevato</b></td><td>{now}</td></tr>
                    </table>
                    <p style="color:#888;font-size:0.85em">DomuWave Health Check — controllo automatico ogni 5 minuti</p>
                    """
            );

            await _emailService.SendAsync(message, smtpConfig, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("[SiteHealthCheckJob] Email di alert inviata a {Email}", notifyEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError("[SiteHealthCheckJob] Invio email fallito: {Error}", ex.Message);
        }
    }

    private SmtpConfig? BuildSmtpConfig()
    {
        var host      = _configuration["DomuWave:SmtpSettings:Host"];
        var portStr   = _configuration["DomuWave:SmtpSettings:Port"];
        var useSslStr = _configuration["DomuWave:SmtpSettings:UseSsl"];
        var username  = _configuration["DomuWave:SmtpSettings:Username"];
        var password  = _configuration["DomuWave:SmtpSettings:Password"];
        var fromEmail = _configuration["DomuWave:SmtpSettings:FromEmail"];
        var fromName  = _configuration["DomuWave:SmtpSettings:FromName"] ?? "DomuWave";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fromEmail))
            return null;

        int.TryParse(portStr, out var port);
        bool.TryParse(useSslStr, out var useSsl);

        return new SmtpConfig(host, port > 0 ? port : 587, useSsl, username, password, fromEmail, fromName);
    }
}
