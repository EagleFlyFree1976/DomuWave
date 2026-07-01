using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.TenantDisplay;
using DomuWave.Services.Dto.TenantDisplay;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── GET ───────────────────────────────────────────────────────────────────────

public class GetTenantDisplaySettingsCommandConsumer
    : InMemoryConsumerBase<GetTenantDisplaySettingsCommand, TenantDisplaySettingsReadDto?>
{
    private readonly IUserService _userService;

    public GetTenantDisplaySettingsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantDisplaySettingsReadDto?> Consume(
        GetTenantDisplaySettingsCommand command,
        IMediationContext               mediationContext,
        CancellationToken               cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // Nessun record → restituisci i default (SoloColore) così il client ha sempre una risposta.
        if (entity == null)
            return new TenantDisplaySettingsReadDto
            {
                AccountingSignConvention     = (int)AccountingSignConvention.SoloColore,
                AccountingSignConventionName = AccountingSignConvention.SoloColore.ToString(),
            };

        return entity.ToReadDto();
    }
}

// ── UPSERT ────────────────────────────────────────────────────────────────────

public class UpsertTenantDisplaySettingsCommandConsumer
    : InMemoryConsumerBase<UpsertTenantDisplaySettingsCommand, TenantDisplaySettingsReadDto>
{
    private readonly IUserService _userService;

    public UpsertTenantDisplaySettingsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantDisplaySettingsReadDto> Consume(
        UpsertTenantDisplaySettingsCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                  cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (!Enum.IsDefined(typeof(AccountingSignConvention), command.Dto.AccountingSignConvention))
            throw new ValidatorException("Convenzione segno non valida.");

        var existing = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (existing == null)
        {
            var tenant = await session.GetAsync<Models.Tenant>(command.TenantId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Tenant non trovato.");

            var entity = command.Dto.ToEntity(tenant);
            entity.Trace(currentUser);
            await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
            return entity.ToReadDto();
        }

        existing.ApplyUpdate(command.Dto);
        existing.Trace(currentUser);
        await session.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return existing.ToReadDto();
    }
}

// ── UPLOAD LOGO ─────────────────────────────────────────────────────────────

public class UploadTenantLogoCommandConsumer
    : InMemoryConsumerBase<UploadTenantLogoCommand, TenantDisplaySettingsReadDto>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/jpg", "image/webp", "image/svg+xml",
    };

    private const int MaxLogoBytes = 512 * 1024; // 512 KB

    private readonly IUserService _userService;

    public UploadTenantLogoCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantDisplaySettingsReadDto> Consume(
        UploadTenantLogoCommand command,
        IMediationContext       mediationContext,
        CancellationToken       cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var dto = command.Dto;
        if (string.IsNullOrWhiteSpace(dto.ContentType) || !AllowedContentTypes.Contains(dto.ContentType.Trim()))
            throw new ValidatorException("Formato immagine non supportato. Ammessi: PNG, JPG, WebP, SVG.");

        if (string.IsNullOrWhiteSpace(dto.Base64Data))
            throw new ValidatorException("Nessun contenuto immagine ricevuto.");

        byte[] content;
        try
        {
            // Rimuove l'eventuale prefisso data URI ("data:image/png;base64,").
            var base64 = dto.Base64Data;
            var comma  = base64.IndexOf(',');
            if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
                base64 = base64[(comma + 1)..];
            content = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new ValidatorException("Contenuto immagine non valido (base64 non leggibile).");
        }

        if (content.Length == 0)
            throw new ValidatorException("Il file immagine è vuoto.");
        if (content.Length > MaxLogoBytes)
            throw new ValidatorException("Il logo supera la dimensione massima di 512 KB.");

        var existing = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (existing == null)
        {
            var tenant = await session.GetAsync<Models.Tenant>(command.TenantId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Tenant non trovato.");

            existing = new TenantDisplaySettings
            {
                Tenant                   = tenant,
                Name                     = "Display",
                AccountingSignConvention = AccountingSignConvention.SoloColore,
            };
            existing.Trace(currentUser);
            ApplyLogo(existing, content, dto);
            await session.SaveAsync(existing, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            ApplyLogo(existing, content, dto);
            existing.Trace(currentUser);
            await session.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return existing.ToReadDto();
    }

    private static void ApplyLogo(TenantDisplaySettings entity, byte[] content, UploadTenantLogoDto dto)
    {
        entity.LogoContent     = content;
        entity.LogoContentType = dto.ContentType.Trim();
        entity.LogoFileName    = dto.FileName;
        entity.LogoUpdatedDate = DateTime.UtcNow;
    }
}

// ── DELETE LOGO ─────────────────────────────────────────────────────────────

public class DeleteTenantLogoCommandConsumer
    : InMemoryConsumerBase<DeleteTenantLogoCommand, TenantDisplaySettingsReadDto>
{
    private readonly IUserService _userService;

    public DeleteTenantLogoCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantDisplaySettingsReadDto> Consume(
        DeleteTenantLogoCommand command,
        IMediationContext       mediationContext,
        CancellationToken       cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var existing = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (existing == null)
            return new TenantDisplaySettingsReadDto
            {
                AccountingSignConvention     = (int)AccountingSignConvention.SoloColore,
                AccountingSignConventionName = AccountingSignConvention.SoloColore.ToString(),
            };

        existing.LogoContent     = null;
        existing.LogoContentType = null;
        existing.LogoFileName    = null;
        existing.LogoUpdatedDate = DateTime.UtcNow;
        existing.Trace(currentUser);
        await session.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return existing.ToReadDto();
    }
}

// ── GET LOGO (contenuto binario) ────────────────────────────────────────────

public class GetTenantLogoCommandConsumer
    : InMemoryConsumerBase<GetTenantLogoCommand, TenantLogoContentDto?>
{
    private readonly IUserService _userService;

    public GetTenantLogoCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<TenantLogoContentDto?> Consume(
        GetTenantLogoCommand command,
        IMediationContext    mediationContext,
        CancellationToken    cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity?.LogoContent == null || entity.LogoContent.Length == 0)
            return null;

        return new TenantLogoContentDto
        {
            Content     = entity.LogoContent,
            ContentType = string.IsNullOrWhiteSpace(entity.LogoContentType)
                ? "application/octet-stream"
                : entity.LogoContentType,
        };
    }
}
