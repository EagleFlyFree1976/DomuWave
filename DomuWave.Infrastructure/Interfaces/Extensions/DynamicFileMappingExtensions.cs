using DomuWave.Services.Dto.DynamicFile;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class DynamicFileMappingExtensions
{
    public static DynamicFileReadDto ToReadDto(this DynamicFile entity, string? base64Data = null)
    {
        if (entity == null) return null;

        var dto = new DynamicFileReadDto
        {
            EntityFullName  = entity.EntityFullName,
            EntityName      = entity.EntityName,
            EntityId        = entity.EntityId,
            FileName        = entity.FileName,
            FileExtension   = entity.FileExtension,
            ContentType     = entity.ContentType,
            FileSize        = entity.FileSize,
            Description     = entity.Description,
            Tags            = entity.Tags,
            StorageTypeId   = entity.StorageType?.Id   ?? 0,
            StorageTypeName = entity.StorageType?.Name ?? string.Empty,
            Base64Data      = base64Data,
        };

        dto.SetTraceInfo(entity);
        return dto;
    }

    public static DynamicFile ToEntity(this UploadDynamicFileDto dto, Tenant tenant, DynamicFileStorageType storageType)
    {
        if (dto == null) return null;

        var extension = Path.GetExtension(dto.FileName);
        var bytes     = Convert.FromBase64String(dto.Base64Data);

        return new DynamicFile
        {
            Tenant          = tenant,
            EntityFullName  = dto.EntityFullName,
            EntityName      = dto.EntityName,
            EntityId        = dto.EntityId,
            Name            = dto.FileName,
            FileName        = dto.FileName,
            FileExtension   = string.IsNullOrWhiteSpace(extension) ? null : extension,
            ContentType     = dto.ContentType,
            FileSize        = bytes.Length,
            Description     = dto.Description,
            Tags            = dto.Tags,
            StorageType     = storageType,
        };
    }

    public static void ApplyUpdate(this DynamicFile entity, UpdateDynamicFileDto dto)
    {
        entity.Description = dto.Description;
        entity.Tags        = dto.Tags;
    }
}
