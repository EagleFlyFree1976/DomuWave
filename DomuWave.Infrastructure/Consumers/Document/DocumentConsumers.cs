using System.IO.Compression;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Document;
using DomuWave.Services.Dto.Document;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Document;

// ── Compression helpers ───────────────────────────────────────────────────────

internal static class DocumentCompression
{
    internal static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gz = new GZipStream(output, CompressionLevel.Optimal))
            gz.Write(data, 0, data.Length);
        return output.ToArray();
    }

    internal static byte[] Decompress(byte[] data)
    {
        // GZip magic bytes: 0x1F 0x8B — skip decompression for legacy uncompressed rows
        if (data.Length < 2 || data[0] != 0x1F || data[1] != 0x8B)
            return data;

        using var input  = new MemoryStream(data);
        using var gz     = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gz.CopyTo(output);
        return output.ToArray();
    }
}

// ── GET BY CONDOMINIUM ────────────────────────────────────────────────────────

public class GetDocumentsByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetDocumentsByCondominiumCommand, IList<DocumentReadDto>>
{
    private readonly IUserService _userService;

    public GetDocumentsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<DocumentReadDto>> Consume(
        GetDocumentsByCondominiumCommand command,
        IMediationContext                 mediationContext,
        CancellationToken                cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<DomuWave.Services.Models.Document>()
            .Where(d => d.Condominium.Id == command.CondominiumId && !d.IsDeleted)
            .OrderByDescending(d => d.DocumentDate)
            .ThenByDescending(d => d.CreationDate)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return list.Select(d => d.ToReadDto()).ToList();
    }
}

// ── GET BY ID ─────────────────────────────────────────────────────────────────

public class GetDocumentByIdCommandConsumer
    : InMemoryConsumerBase<GetDocumentByIdCommand, DocumentReadDto?>
{
    private readonly IUserService _userService;

    public GetDocumentByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<DocumentReadDto?> Consume(
        GetDocumentByIdCommand command,
        IMediationContext       mediationContext,
        CancellationToken      cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<DomuWave.Services.Models.Document>()
            .FirstOrDefaultAsync(d => d.Id == command.Id && !d.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        return entity?.ToReadDto();
    }
}

// ── DOWNLOAD ─────────────────────────────────────────────────────────────────

public class DownloadDocumentCommandConsumer
    : InMemoryConsumerBase<DownloadDocumentCommand, (byte[] Content, string FileName, string MimeType)>
{
    private readonly IUserService _userService;

    public DownloadDocumentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<(byte[] Content, string FileName, string MimeType)> Consume(
        DownloadDocumentCommand command,
        IMediationContext        mediationContext,
        CancellationToken       cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<DomuWave.Services.Models.Document>()
            .FirstOrDefaultAsync(d => d.Id == command.Id && !d.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Documento non trovato.");

        var content = await session.Query<DocumentContent>()
            .FirstOrDefaultAsync(c => c.Document.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Contenuto del documento non trovato.");

        return (DocumentCompression.Decompress(content.FileContent), entity.FileName, entity.MimeType ?? "application/octet-stream");
    }
}

// ── UPLOAD ────────────────────────────────────────────────────────────────────

public class UploadDocumentCommandConsumer
    : InMemoryConsumerBase<UploadDocumentCommand, DocumentReadDto>
{
    private readonly IUserService          _userService;
    private readonly ICondominiumService   _condominiumService;

    public UploadDocumentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        ICondominiumService     condominiumService) : base(sessionFactoryProvider)
    {
        _userService        = userService;
        _condominiumService = condominiumService;
    }

    protected override async Task<DocumentReadDto> Consume(
        UploadDocumentCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser  = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var condominium  = await _condominiumService.GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken).ConfigureAwait(false)
                           ?? throw new NotFoundException("Condominio non trovato.");

        if (string.IsNullOrWhiteSpace(command.Dto.Title) && command.Dto.EntityId == null)
            throw new ValidatorException("Il titolo è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            command.Dto.Title = command.Dto.FileName;
        if (string.IsNullOrWhiteSpace(command.Dto.FileContent))
            throw new ValidatorException("Il file è obbligatorio.");

        var fileBytes = Convert.FromBase64String(command.Dto.FileContent);

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);

        var contentEntity = new DocumentContent
        {
            Tenant      = condominium.Tenant,
            Document    = entity,
            FileContent = DocumentCompression.Compress(fileBytes),
        };
        contentEntity.Trace(currentUser);
        await session.SaveAsync(contentEntity, cancellationToken).ConfigureAwait(false);

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── UPDATE METADATA ───────────────────────────────────────────────────────────

public class UpdateDocumentCommandConsumer
    : InMemoryConsumerBase<UpdateDocumentCommand, DocumentReadDto?>
{
    private readonly IUserService _userService;

    public UpdateDocumentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<DocumentReadDto?> Consume(
        UpdateDocumentCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<DomuWave.Services.Models.Document>()
            .FirstOrDefaultAsync(d => d.Id == command.Id && !d.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Documento non trovato.");

        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── GET BY ENTITY ─────────────────────────────────────────────────────────────

public class GetDocumentsByEntityCommandConsumer
    : InMemoryConsumerBase<GetDocumentsByEntityCommand, IList<DocumentReadDto>>
{
    private readonly IUserService _userService;

    public GetDocumentsByEntityCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<DocumentReadDto>> Consume(
        GetDocumentsByEntityCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<DomuWave.Services.Models.Document>()
            .Where(d => d.EntityId == command.EntityId
                     && d.EntityFullName == command.EntityFullName
                     && !d.IsDeleted)
            .OrderByDescending(d => d.CreationDate)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return list.Select(d => d.ToReadDto()).ToList();
    }
}

// ── DELETE ────────────────────────────────────────────────────────────────────

public class DeleteDocumentCommandConsumer
    : InMemoryConsumerBase<DeleteDocumentCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteDocumentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        DeleteDocumentCommand command,
        IMediationContext      mediationContext,
        CancellationToken     cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<DomuWave.Services.Models.Document>()
            .FirstOrDefaultAsync(d => d.Id == command.Id && !d.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        var content = await session.Query<DocumentContent>()
            .FirstOrDefaultAsync(c => c.Document.Id == command.Id, cancellationToken)
            .ConfigureAwait(false);
        if (content != null)
        {
            content.IsDeleted = true;
            content.Trace(currentUser);
            await session.UpdateAsync(content, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
