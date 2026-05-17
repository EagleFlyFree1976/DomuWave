using CPQ.Core.Extensions;
using DomuWave.Services.Dto.NotificationAttachment;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class NotificationAttachmentMappingExtensions
{
    public static NotificationAttachmentReadDto ToReadDto(this NotificationAttachment entity)
    {
        if (entity == null) return null!;
        var dto = new NotificationAttachmentReadDto
        {
            NotificationId = entity.Notification?.Id ?? 0,
            DocumentId     = entity.Document?.Id ?? 0,
            DocumentTitle  = entity.Document?.Title ?? string.Empty,
            FileName       = entity.Document?.FileName ?? string.Empty,
            MimeType       = entity.Document?.MimeType,
            FileSize       = entity.Document?.FileSize ?? 0,
            Category       = entity.Document?.Category,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }
}
