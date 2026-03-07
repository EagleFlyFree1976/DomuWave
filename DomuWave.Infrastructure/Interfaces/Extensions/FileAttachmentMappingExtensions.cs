using DomuWave.Services.Dto.FileAttachment;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class FileAttachmentMappingExtensions
{
    public static FileAttachmentReadDto ToReadDto(this FileAttachment entity, bool includeContent = false)
    {
        var dto = new FileAttachmentReadDto
        {
            Id          = entity.Id,
            EntityKey   = entity.EntityKey,
            EntityId    = entity.EntityId,
            FileName    = entity.FileName,
            ContentType = entity.ContentType,
            FileSize    = entity.FileSize,
            CreatedAt   = entity.CreationDate,
        };

        if (includeContent && entity.Content != null)
            dto.Base64Data = entity.Content.Base64Data;

        return dto;
    }

    public static FileAttachment ToEntity(this UploadFileAttachmentDto dto, Tenant tenant)
    {
        var bytes = Convert.FromBase64String(dto.Base64Data);

        var attachment = new FileAttachment
        {
            EntityKey   = dto.EntityKey,
            EntityId    = dto.EntityId,
            FileName    = dto.FileName,
            ContentType = dto.ContentType,
            FileSize    = bytes.Length,
            Tenant      = tenant,
        };

        attachment.Content = new FileAttachmentContent
        {
            Base64Data = dto.Base64Data,
            Attachment = attachment,
        };

        return attachment;
    }
}
