using System.Net;
using System.Net.Mail;
using CPQ.Core.Exceptions;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations;

public class SmtpEmailService : IEmailService
{
    public async Task SendAsync(EmailMessage message, SmtpConfig config, CancellationToken ct = default)
    {
        if (message == null)
            throw new ValidatorException("Messaggio email non valido.");

        if (config == null)
            throw new ValidatorException("Configurazione SMTP non trovata. Configura il server email nelle impostazioni.");

        if (string.IsNullOrWhiteSpace(config.Host)
            || config.Port <= 0
            || string.IsNullOrWhiteSpace(config.FromEmail)
            || string.IsNullOrWhiteSpace(config.Username)
            || string.IsNullOrWhiteSpace(config.Password))
        {
            throw new ValidatorException("Configurazione SMTP incompleta. Verifica host, porta, credenziali e mittente.");
        }

        if (string.IsNullOrWhiteSpace(message.To))
            throw new ValidatorException("Indirizzo email destinatario non valido.");

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

        if (message.Attachments != null)
        {
            foreach (var att in message.Attachments)
            {
                var stream = new MemoryStream(att.Content);
                mail.Attachments.Add(new Attachment(stream, att.FileName, att.ContentType));
            }
        }

        try
        {
            await client.SendMailAsync(mail, ct).ConfigureAwait(false);
        }
        catch (SmtpException ex)
        {
            throw new ValidatorException($"Invio email fallito: {ex.Message}");
        }
    }
}
