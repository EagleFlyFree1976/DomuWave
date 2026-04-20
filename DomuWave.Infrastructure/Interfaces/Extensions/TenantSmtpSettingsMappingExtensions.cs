using CPQ.Core.Extensions;
using DomuWave.Services.Dto.TenantSmtp;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class TenantSmtpSettingsMappingExtensions
{
    public static TenantSmtpSettingsReadDto ToReadDto(this TenantSmtpSettings entity)
    {
        if (entity == null) return null!;
        var dto = new TenantSmtpSettingsReadDto
        {
            Host      = entity.Host,
            Port      = entity.Port,
            UseSsl    = entity.UseSsl,
            Username  = entity.Username,
            FromEmail = entity.FromEmail,
            FromName  = entity.FromName,
            IsEnabled = entity.IsEnabled,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static TenantSmtpSettings ToEntity(this UpsertTenantSmtpSettingsDto dto, Tenant tenant, string encryptedPassword)
    {
        if (dto == null) return null!;
        return new TenantSmtpSettings
        {
            Tenant            = tenant,
            Name              = "SMTP",
            Host              = dto.Host,
            Port              = dto.Port,
            UseSsl            = dto.UseSsl,
            Username          = dto.Username,
            PasswordEncrypted = encryptedPassword,
            FromEmail         = dto.FromEmail,
            FromName          = dto.FromName,
            IsEnabled         = dto.IsEnabled,
        };
    }

    public static void ApplyUpdate(this TenantSmtpSettings entity, UpsertTenantSmtpSettingsDto dto, string? encryptedPassword)
    {
        entity.Host      = dto.Host;
        entity.Port      = dto.Port;
        entity.UseSsl    = dto.UseSsl;
        entity.Username  = dto.Username;
        entity.FromEmail = dto.FromEmail;
        entity.FromName  = dto.FromName;
        entity.IsEnabled = dto.IsEnabled;
        if (encryptedPassword != null)
            entity.PasswordEncrypted = encryptedPassword;
    }
}
