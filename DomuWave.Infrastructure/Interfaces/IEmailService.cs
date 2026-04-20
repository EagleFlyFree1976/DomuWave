namespace DomuWave.Services.Interfaces;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, SmtpConfig config, CancellationToken ct = default);
}

public record SmtpConfig(
    string Host,
    int    Port,
    bool   UseSsl,
    string Username,
    string Password,
    string FromEmail,
    string FromName);

public record EmailMessage(
    string   To,
    string   ToName,
    string   Subject,
    string   BodyHtml);
