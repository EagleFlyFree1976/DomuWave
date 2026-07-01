using CPQ.Core.Extensions;
using DomuWave.Services.Dto.TenantDisplay;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class TenantDisplaySettingsMappingExtensions
{
    public static TenantDisplaySettingsReadDto ToReadDto(this TenantDisplaySettings entity)
    {
        if (entity == null) return null!;
        var hasLogo = entity.LogoContent != null && entity.LogoContent.Length > 0;
        var dto = new TenantDisplaySettingsReadDto
        {
            AccountingSignConvention     = (int)entity.AccountingSignConvention,
            AccountingSignConventionName = entity.AccountingSignConvention.ToString(),
            HasLogo                      = hasLogo,
            LogoContentType              = hasLogo ? entity.LogoContentType : null,
            LogoUrl                      = hasLogo
                ? $"/api/tenant-display/logo?v={(entity.LogoUpdatedDate ?? entity.CreationDate).Ticks}"
                : null,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static TenantDisplaySettings ToEntity(this UpsertTenantDisplaySettingsDto dto, Tenant tenant)
    {
        if (dto == null) return null!;
        return new TenantDisplaySettings
        {
            Tenant                   = tenant,
            Name                     = "Display",
            AccountingSignConvention = (AccountingSignConvention)dto.AccountingSignConvention,
        };
    }

    public static void ApplyUpdate(this TenantDisplaySettings entity, UpsertTenantDisplaySettingsDto dto)
    {
        entity.AccountingSignConvention = (AccountingSignConvention)dto.AccountingSignConvention;
    }
}
