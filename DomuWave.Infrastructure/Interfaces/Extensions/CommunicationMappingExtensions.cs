using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Communication;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class CommunicationMappingExtensions
{
    public static CommunicationReadDto ToReadDto(this Communication entity)
    {
        if (entity == null) return null!;
        var dto = new CommunicationReadDto
        {
            CondominiumId     = entity.Condominium?.Id   ?? 0,
            CondominiumName   = entity.Condominium?.Name,
            InstallmentId     = entity.Installment?.Id,
            AssemblyId        = entity.Assembly?.Id,
            AssemblyTitle     = entity.Assembly?.Name,
            Title             = entity.Title,
            Content           = entity.Content,
            CommunicationType = entity.CommunicationType,
            Priority          = entity.Priority,
            PublicationDate   = entity.PublicationDate,
            ExpirationDate    = entity.ExpirationDate,
            SendEmail         = entity.SendEmail,
            EmailSentAt       = entity.EmailSentAt,
            IsVisible         = entity.IsVisible,
            AttachmentPath    = entity.AttachmentPath,
            IsArchived        = entity.IsArchived,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Communication ToEntity(this CreateCommunicationDto dto, Condominium condominium, CondominiumInstallment? installment = null, Assembly? assembly = null)
    {
        if (dto == null) return null!;
        return new Communication
        {
            Condominium       = condominium,
            Tenant            = condominium.Tenant,
            Installment       = installment,
            Assembly          = assembly,
            Name              = dto.Title,
            Description       = dto.Content,
            CommunicationType = dto.CommunicationType,
            Priority          = dto.Priority,
            PublicationDate   = dto.PublicationDate,
            ExpirationDate    = dto.ExpirationDate,
            SendEmail         = dto.SendEmail,
            IsVisible         = dto.IsVisible,
            AttachmentPath    = dto.AttachmentPath,
        };
    }

    public static void ApplyUpdate(this Communication entity, UpdateCommunicationDto dto)
    {
        if (dto.Title   != null) entity.Name        = dto.Title;
        if (dto.Content != null) entity.Description = dto.Content;
        if (dto.CommunicationType != null) entity.CommunicationType = dto.CommunicationType;
        if (dto.Priority          != null) entity.Priority          = dto.Priority;
        if (dto.PublicationDate   != null) entity.PublicationDate   = dto.PublicationDate.Value;
        if (dto.ExpirationDate    != null) entity.ExpirationDate    = dto.ExpirationDate;
        if (dto.SendEmail         != null) entity.SendEmail         = dto.SendEmail.Value;
        if (dto.IsVisible         != null) entity.IsVisible         = dto.IsVisible.Value;
        if (dto.AttachmentPath    != null) entity.AttachmentPath    = dto.AttachmentPath;
    }
}
