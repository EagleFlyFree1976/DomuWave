using CPQ.Core.Extensions;
using DomuWave.Services.Dto.NotificationTemplate;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class NotificationTemplateMappingExtensions
{
    public static NotificationTemplateReadDto ToReadDto(this NotificationTemplate entity)
    {
        if (entity == null) return null!;
        var dto = new NotificationTemplateReadDto
        {
            CondominiumId     = entity.Condominium?.Id   ?? 0,
            CondominiumName   = entity.Condominium?.Name,
            Name              = entity.Name,
            CommunicationType = entity.CommunicationType,
            SubjectTemplate   = entity.SubjectTemplate,
            BodyTemplate      = entity.BodyTemplate,
            IsDefault         = entity.IsDefault,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static NotificationTemplate ToEntity(this CreateNotificationTemplateDto dto, Condominium condominium, Tenant tenant)
    {
        if (dto == null) return null!;
        return new NotificationTemplate
        {
            Tenant            = tenant,
            Condominium       = condominium,
            Name              = dto.Name,
            CommunicationType = dto.CommunicationType,
            SubjectTemplate   = dto.SubjectTemplate,
            BodyTemplate      = dto.BodyTemplate,
            IsDefault         = dto.IsDefault,
        };
    }

    public static void ApplyUpdate(this NotificationTemplate entity, UpdateNotificationTemplateDto dto)
    {
        if (dto.Name            != null) entity.Name            = dto.Name;
        if (dto.SubjectTemplate != null) entity.SubjectTemplate = dto.SubjectTemplate;
        if (dto.BodyTemplate    != null) entity.BodyTemplate    = dto.BodyTemplate;
        if (dto.IsDefault       != null) entity.IsDefault       = dto.IsDefault.Value;
    }
}
