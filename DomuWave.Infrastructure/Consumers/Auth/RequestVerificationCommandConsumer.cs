using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Command.Auth;
using DomuWave.Services.Dto.Auth;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Auth;

public class RequestVerificationCommandConsumer
    : InMemoryConsumerBase<RequestVerificationCommand, RequestVerificationResultDto>
{
    private readonly IPendingRegistrationService _pendingService;
    private readonly IEmailService               _emailService;
    private readonly DomuWaveSettings             _settings;
    private readonly ILogger<RequestVerificationCommandConsumer> _logger;

    public RequestVerificationCommandConsumer(
        ISessionFactoryProvider          sessionFactoryProvider,
        IPendingRegistrationService      pendingService,
        IEmailService                    emailService,
        IOptionsMonitor<DomuWaveSettings> settings,
        ILogger<RequestVerificationCommandConsumer> logger) : base(sessionFactoryProvider)
    {
        _pendingService = pendingService;
        _emailService   = emailService;
        _settings       = settings.CurrentValue;
        _logger         = logger;
    }

    protected override async Task<RequestVerificationResultDto> Consume(
        RequestVerificationCommand command,
        IMediationContext          mediationContext,
        CancellationToken          cancellationToken)
    {
        var pending = await _pendingService.GetByIdAsync(command.RegistrationId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Registrazione non trovata.");

        if (pending.Status != PendingRegistrationStatus.Pending)
            throw new ValidatorException("La registrazione è già stata confermata o è scaduta.");

        // Salva i dati del condominio + genera token di verifica
        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        pending.VerificationToken     = token;
        pending.VerificationExpiresAt = DateTime.UtcNow.AddDays(2);
        pending.CondominiumName       = command.CondominiumName;
        pending.CondominiumCode       = command.CondominiumCode;
        pending.CondominiumCity       = command.CondominiumCity;
        pending.CondominiumZip        = command.CondominiumZip;
        await _pendingService.UpdateAsync(pending, cancellationToken).ConfigureAwait(false);

        // Costruisce il link e invia la mail
        var baseUrl = string.IsNullOrWhiteSpace(_settings.RegistrationVerifyUrl)
            ? "http://localhost:40300/register/verify"
            : _settings.RegistrationVerifyUrl;
        var link = $"{baseUrl}?token={token}";

        var smtp = _settings.SmtpSettings;
        if (smtp is null || string.IsNullOrWhiteSpace(smtp.Host))
        {
            // SMTP non configurato: logga il link così il flusso è testabile in dev
            _logger.LogWarning("SMTP non configurato. Link di verifica per {Email}: {Link}", pending.Email, link);
            return new RequestVerificationResultDto { Email = pending.Email };
        }

        var config = new SmtpConfig(smtp.Host, smtp.Port, smtp.UseSsl, smtp.Username, smtp.Password, smtp.FromEmail, smtp.FromName);
        var body = $@"
            <div style='font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto'>
              <h2 style='color:#2563eb'>Conferma la tua registrazione</h2>
              <p>Ciao,<br>per completare la registrazione del tuo account DomuWave clicca il pulsante qui sotto.</p>
              <p style='text-align:center;margin:28px 0'>
                <a href='{link}' style='background:#2563eb;color:#fff;padding:12px 28px;border-radius:8px;text-decoration:none;font-weight:600'>Conferma registrazione</a>
              </p>
              <p style='font-size:13px;color:#666'>Oppure copia questo link nel browser:<br><a href='{link}'>{link}</a></p>
              <p style='font-size:12px;color:#999'>Il link scade tra 48 ore. Se non hai richiesto questa registrazione, ignora questa email.</p>
            </div>";

        var msg = new EmailMessage(pending.Email, pending.Email, "Conferma la tua registrazione DomuWave", body);

        try
        {
            await _emailService.SendAsync(msg, config, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invio email di verifica fallito per {Email}. Link: {Link}", pending.Email, link);
            throw new ValidatorException("Impossibile inviare l'email di verifica. Riprova più tardi.");
        }

        return new RequestVerificationResultDto { Email = pending.Email };
    }
}
