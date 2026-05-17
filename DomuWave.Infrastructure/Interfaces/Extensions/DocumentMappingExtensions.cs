using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Document;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class DocumentMappingExtensions
{
    public static DocumentReadDto ToReadDto(this Document entity)
    {
        if (entity == null) return null!;
        var dto = new DocumentReadDto
        {
            CondominiumId     = entity.Condominium?.Id   ?? 0,
            CondominiumName   = entity.Condominium?.Name ?? string.Empty,
            Title             = entity.Title,
            Category          = entity.Category,
            FileName          = entity.FileName,
            FileSize          = entity.FileSize,
            MimeType          = entity.MimeType,
            DocumentDate      = entity.DocumentDate,
            IsVisibleToOwners = entity.IsVisibleToOwners,
            IsArchived        = entity.IsArchived,
            Tags              = entity.Tags,
            Description       = entity.Description,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Document ToEntity(this UploadDocumentDto dto, Condominium condominium, Tenant tenant)
    {
        if (dto == null) return null!;
        return new Document
        {
            Condominium       = condominium,
            Tenant            = tenant,
                Name = dto.FileName,
            Title             = dto.Title,
            Category          = dto.Category ?? "Altro",
            FileName          = dto.FileName,
            FilePath          = string.Empty,
            FileSize          = dto.FileSize,
            MimeType          = dto.MimeType ?? "application/octet-stream",
            DocumentDate      = dto.DocumentDate,
            IsVisibleToOwners = dto.IsVisibleToOwners,
            IsArchived        = false,
            Tags              = dto.Tags,
            Description       = dto.Description,
        };
    }

    public static void ApplyUpdate(this Document entity, UpdateDocumentDto dto)
    {
        if (dto.Title             != null) entity.Title             = dto.Title;
        if (dto.Category          != null) entity.Category          = dto.Category;
        if (dto.DocumentDate      != null) entity.DocumentDate      = dto.DocumentDate;
        if (dto.IsVisibleToOwners != null) entity.IsVisibleToOwners = dto.IsVisibleToOwners.Value;
        if (dto.IsArchived        != null) entity.IsArchived        = dto.IsArchived.Value;
        if (dto.Tags              != null) entity.Tags              = dto.Tags;
        if (dto.Description       != null) entity.Description       = dto.Description;
    }
}
