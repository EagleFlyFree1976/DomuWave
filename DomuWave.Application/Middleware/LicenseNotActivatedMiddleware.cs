using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using System.Net;
using System.Text.Json;

namespace DomuWave.Application.Middleware;

/// <summary>
/// Intercetta le risposte HTTP 402 prodotte da [RequiresFeature].
/// Se il motivo è la mancanza di licenza:
///   1. Invia una mail all'owner del tenant con il link alla pagina piani
///   2. Sovrascrive la risposta con { reason: "LICENSE_NOT_ACTIVATED" }
///      in modo che il frontend mostri la pagina "Servizio non attivato"
/// </summary>
public class LicenseNotActivatedMiddleware(
    ILogger<LicenseNotActivatedMiddleware> logger,
    IEmailService                          emailService,
    IConfiguration                         configuration,
    IUserService                           userService,
    ITenantService                         tenantService,
    ISessionFactoryProvider                sessionFactoryProvider) : IMiddleware
{
    // Evita di inviare la stessa mail più volte per lo stesso tenant nella stessa ora
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, DateTime> _mailSent = new();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBody = context.Response.Body;
        using var buffer = new System.IO.MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        if (context.Response.StatusCode != (int)HttpStatusCode.PaymentRequired)
        {
            buffer.Seek(0, System.IO.SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody, context.RequestAborted);
            return;
        }

        var tenantIdStr = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (Guid.TryParse(tenantIdStr, out var tenantId))
            await TrySendActivationMailAsync(tenantId, context.RequestAborted);

        var json = JsonSerializer.Serialize(new
        {
            reason  = "LICENSE_NOT_ACTIVATED",
            message = "Il servizio non è attivo per questo tenant. Acquista una licenza per continuare."
        });

        context.Response.ContentType   = "application/json";
        context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(json);

        await originalBody.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json), context.RequestAborted);
    }

    private async Task TrySendActivationMailAsync(Guid tenantId, CancellationToken ct)
    {
        // Throttle: max 1 mail per tenant per ora
        if (_mailSent.TryGetValue(tenantId, out var lastSent) &&
            DateTime.UtcNow - lastSent < TimeSpan.FromHours(1))
            return;

        try
        {
            var tenant = await tenantService.GetByIdAsync(tenantId, null!, ct).ConfigureAwait(false);
            if (tenant == null) return;

            var owner = await userService.GetByIdAsync(tenant.CreatedBy, ct).ConfigureAwait(false);
            if (owner == null || string.IsNullOrWhiteSpace(owner.Email)) return;

            var smtpConfig = await BuildSmtpConfigAsync(tenantId, ct).ConfigureAwait(false);
            if (smtpConfig == null)
            {
                logger.LogWarning("[LicenseNotActivated] SMTP non configurato — mail saltata per tenant {TenantId}", tenantId);
                return;
            }

            var appBaseUrl = configuration["AppBaseUrl"] ?? string.Empty;
            var plansUrl   = $"{appBaseUrl.TrimEnd('/')}/servizio-non-attivato";

            var body = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
                  <h2 style="color:#6366f1">Attiva il servizio DomuWave</h2>
                  <p>Gentile <strong>{owner.Name} {owner.LastName}</strong>,</p>
                  <p>
                    È stato rilevato un accesso a funzionalità di <strong>DomuWave</strong>
                    non ancora attivate per il tuo account (<em>{tenant.Name}</em>).
                  </p>
                  <p>Per continuare a utilizzare il servizio, acquista una licenza:</p>
                  <p style="text-align:center;margin:32px 0">
                    <a href="{plansUrl}"
                       style="background:#6366f1;color:#fff;padding:14px 32px;border-radius:8px;
                              text-decoration:none;font-weight:bold;font-size:16px">
                      Visualizza piani e prezzi
                    </a>
                  </p>
                  <p style="color:#888;font-size:0.85em">
                    Questo messaggio è stato generato automaticamente da DomuWave.
                  </p>
                </div>
                """;

            var message = new EmailMessage(
                To:       owner.Email,
                ToName:   $"{owner.Name} {owner.LastName}".Trim(),
                Subject:  "DomuWave — Attiva il tuo servizio",
                BodyHtml: body);

            await emailService.SendAsync(message, smtpConfig, ct).ConfigureAwait(false);

            _mailSent[tenantId] = DateTime.UtcNow;
            logger.LogInformation("[LicenseNotActivated] Mail di attivazione inviata a {Email} per tenant {TenantId}",
                owner.Email, tenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[LicenseNotActivated] Errore invio mail per tenant {TenantId}", tenantId);
        }
    }

    private async Task<SmtpConfig?> BuildSmtpConfigAsync(Guid tenantId, CancellationToken ct)
    {
        var session = sessionFactoryProvider.GetSession(null);

        var settings = await session.Query<TenantSmtpSettings>()
            .Where(s => s.Tenant.Id == tenantId && !s.IsDeleted && s.IsEnabled)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (settings == null) return null;

        var password = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(settings.PasswordEncrypted));
        return new SmtpConfig(settings.Host, settings.Port, settings.UseSsl,
                              settings.Username, password,
                              settings.FromEmail, settings.FromName);
    }
}
