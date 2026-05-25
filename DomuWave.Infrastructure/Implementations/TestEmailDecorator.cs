using DomuWave.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DomuWave.Services.Implementations;

/// <summary>
/// Decorator di IEmailService che, quando DomuWave:TestEmail è configurato,
/// redirige ogni mail all'indirizzo di test e aggiunge un banner con il destinatario reale.
/// </summary>
public class TestEmailDecorator(IEmailService inner, IConfiguration configuration) : IEmailService
{
    private readonly string? _testEmail = string.IsNullOrWhiteSpace(configuration["DomuWave:TestEmail"])
        ? null
        : configuration["DomuWave:TestEmail"]!.Trim();

    public async Task SendAsync(EmailMessage message, SmtpConfig config, CancellationToken ct = default)
    {
        if (_testEmail == null)
        {
            await inner.SendAsync(message, config, ct).ConfigureAwait(false);
            return;
        }

        var testBanner =
            $"<div style=\"background:#fff3cd;border:1px solid #ffc107;padding:12px 16px;margin-bottom:20px;font-family:sans-serif;font-size:13px;\">" +
            $"<strong>&#9888; MODALITÀ TEST</strong><br/>" +
            $"Questa email sarebbe stata inviata a: <strong>{message.To}</strong> ({message.ToName})" +
            $"</div><hr style=\"margin-bottom:20px\"/>";

        var redirected = message with
        {
            To       = _testEmail,
            ToName   = $"[TEST] {message.ToName}",
            BodyHtml = testBanner + message.BodyHtml,
        };

        await inner.SendAsync(redirected, config, ct).ConfigureAwait(false);
    }
}
