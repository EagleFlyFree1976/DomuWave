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

// ── BRANDING: UPLOAD LOGO ───────────────────────────────────────────────────────

public class UploadTenantLogoCommandConsumer
    : InMemoryConsumerBase<UploadTenantLogoCommand, TenantDisplaySettingsReadDto>
{
    private const int MaxLogoBytes = 512 * 1024; // 512 KB
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/webp", "image/svg+xml",
    };

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

        var contentType = command.Dto.ContentType?.Trim() ?? string.Empty;
        if (!AllowedContentTypes.Contains(contentType))
            throw new ValidatorException("Formato immagine non supportato. Usa PNG, JPG, WebP o SVG.");

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(command.Dto.Base64Data ?? string.Empty);
        }
        catch (FormatException)
        {
            throw new ValidatorException("Immagine non valida.");
        }

        if (bytes.Length == 0)
            throw new ValidatorException("Immagine vuota.");
        if (bytes.Length > MaxLogoBytes)
            throw new ValidatorException("Il logo non può superare 512 KB.");

        var entity = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null)
        {
            var tenant = await session.GetAsync<Models.Tenant>(command.TenantId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Tenant non trovato.");
            entity = new TenantDisplaySettings { Tenant = tenant, Name = "Display" };
        }

        entity.LogoContent     = bytes;
        entity.LogoContentType = contentType;
        entity.LogoFileName    = command.Dto.FileName;
        entity.LogoUpdatedDate = DateTime.UtcNow;
        entity.Trace(currentUser);

        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── BRANDING: DELETE LOGO ───────────────────────────────────────────────────────

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

        var entity = await session.Query<TenantDisplaySettings>()
            .Where(s => s.Tenant.Id == command.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // Nessun record o nessun logo → niente da fare, ritorna lo stato corrente (o default).
        if (entity == null)
            return new TenantDisplaySettingsReadDto
            {
                AccountingSignConvention     = (int)AccountingSignConvention.SoloColore,
                AccountingSignConventionName = AccountingSignConvention.SoloColore.ToString(),
            };

        entity.LogoContent     = null;
        entity.LogoContentType = null;
        entity.LogoFileName    = null;
        entity.LogoUpdatedDate = DateTime.UtcNow;
        entity.Trace(currentUser);

        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── BRANDING: GET LOGO (contenuto) ──────────────────────────────────────────────

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

        if (entity?.LogoContent is not { Length: > 0 })
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
