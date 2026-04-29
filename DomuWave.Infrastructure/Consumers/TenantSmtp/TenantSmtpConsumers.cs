using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.TenantSmtp;
using DomuWave.Services.Dto.TenantSmtp;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── GET ───────────────────────────────────────────────────────────────────────

public class GetTenantSmtpSettingsCommandConsumer
    : InMemoryConsumerBase<GetTenantSmtpSettingsCommand, TenantSmtpSettingsReadDto?>
{
    private readonly IUserService _userService;

    public GetTenantSmtpSettingsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantSmtpSettingsReadDto?> Consume(
        GetTenantSmtpSettingsCommand command,
        IMediationContext             mediationContext,
        CancellationToken            cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<TenantSmtpSettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return entity?.ToReadDto();
    }
}

// ── UPSERT ────────────────────────────────────────────────────────────────────

public class UpsertTenantSmtpSettingsCommandConsumer
    : InMemoryConsumerBase<UpsertTenantSmtpSettingsCommand, TenantSmtpSettingsReadDto>
{
    private readonly IUserService _userService;

    public UpsertTenantSmtpSettingsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantSmtpSettingsReadDto> Consume(
        UpsertTenantSmtpSettingsCommand command,
        IMediationContext                mediationContext,
        CancellationToken               cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Host))
            throw new ValidatorException("Il server SMTP è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.FromEmail))
            throw new ValidatorException("L'indirizzo mittente è obbligatorio.");

        var existing = await session.Query<TenantSmtpSettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // Encrypt password with simple Base64 (replace with proper encryption if needed)
        string? encryptedPwd = command.Dto.Password != null
            ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(command.Dto.Password))
            : null;

        if (existing == null)
        {
            if (string.IsNullOrWhiteSpace(command.Dto.Password))
                throw new ValidatorException("La password SMTP è obbligatoria per la prima configurazione.");

            var tenant = await session.GetAsync<Models.Tenant>(command.TenantId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Tenant non trovato.");

            var entity = command.Dto.ToEntity(tenant, encryptedPwd!);
            entity.Trace(currentUser);
            await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
            return entity.ToReadDto();
        }
        else
        {
            existing.ApplyUpdate(command.Dto, encryptedPwd);
            existing.Trace(currentUser);
            await session.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
            return existing.ToReadDto();
        }
    }
}

// ── TEST ──────────────────────────────────────────────────────────────────────

public class TestTenantSmtpCommandConsumer
    : InMemoryConsumerBase<TestTenantSmtpCommand, bool>
{
    private readonly IUserService  _userService;
    private readonly IEmailService _emailService;

    public TestTenantSmtpCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IEmailService           emailService) : base(sessionFactoryProvider)
    {
        _userService  = userService;
        _emailService = emailService;
    }

    protected override async Task<bool> Consume(
        TestTenantSmtpCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var settings = await session.Query<TenantSmtpSettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Configurazione SMTP non trovata.");

        var password = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(settings.PasswordEncrypted));

        var config = new SmtpConfig(
            settings.Host, settings.Port, settings.UseSsl,
            settings.Username, password,
            settings.FromEmail, settings.FromName);

        var msg = new EmailMessage(
            To: command.TestEmailTo,
            ToName: "Test",
            Subject: "DomuWave — Test configurazione SMTP",
            BodyHtml: "<p>La configurazione SMTP è funzionante.</p>");

        await _emailService.SendAsync(msg, config, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
