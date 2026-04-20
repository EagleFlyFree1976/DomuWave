using CPQ.Core.Extensions;
using DomuWave.Services.Dto.CommunicationNotification;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class CommunicationNotificationMappingExtensions
{
    private static readonly string[] DeliveryMethodNames = ["Email", "Raccomandata"];
    private static readonly string[] StatusNames         = ["Bozza", "Pianificata", "Inviata", "Consegnata", "Fallita", "Stampata"];

    public static CommunicationNotificationReadDto ToReadDto(this CommunicationNotification entity)
    {
        if (entity == null) return null!;
        var dto = new CommunicationNotificationReadDto
        {
            CommunicationId    = entity.Communication?.Id ?? 0,
            CommunicationTitle = entity.Communication?.Title,
            RecipientUserId    = entity.RecipientUserId,
            RecipientFullName  = entity.RecipientFullName,
            UnitId             = entity.Unit?.Id,
            UnitDisplayName    = entity.Unit?.DisplayName ?? entity.Unit?.InternalNumber,
            DeliveryMethod     = entity.DeliveryMethod,
            DeliveryMethodName = entity.DeliveryMethod < DeliveryMethodNames.Length
                                     ? DeliveryMethodNames[entity.DeliveryMethod] : string.Empty,
            Status             = entity.Status,
            StatusName         = entity.Status < StatusNames.Length
                                     ? StatusNames[entity.Status] : string.Empty,
            ScheduledAt        = entity.ScheduledAt,
            SentAt             = entity.SentAt,
            DeliveredAt        = entity.DeliveredAt,
            PrintedAt          = entity.PrintedAt,
            TrackingNumber     = entity.TrackingNumber,
            ErrorMessage       = entity.ErrorMessage,
            EmailAddress       = entity.EmailAddress,
            PostalAddress      = entity.PostalAddress,
            SubjectResolved    = entity.SubjectResolved,
            BodyResolved       = entity.BodyResolved,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }
}
