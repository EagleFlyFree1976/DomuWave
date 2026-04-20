using System.Net;
using System.Net.Mail;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations;

public class SmtpEmailService : IEmailService
{
    public async Task SendAsync(EmailMessage message, SmtpConfig config, CancellationToken ct = default)
    {
        using var client = new SmtpClient(config.Host, config.Port)
        {
            EnableSsl   = config.UseSsl,
            Credentials = new NetworkCredential(config.Username, config.Password),
        };

        var mail = new MailMessage
        {
            From       = new MailAddress(config.FromEmail, config.FromName),
            Subject    = message.Subject,
            Body       = message.BodyHtml,
            IsBodyHtml = true,
        };
        mail.To.Add(new MailAddress(message.To, message.ToName));

        await client.SendMailAsync(mail, ct).ConfigureAwait(false);
    }
}
