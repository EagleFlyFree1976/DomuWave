using DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ChartOfAccountsCategoryTemplateMappingExtensions
{
    public static ChartOfAccountsCategoryTemplateReadDto ToReadDto(this ChartOfAccountsCategoryTemplate template)
    {
        if (template == null) return null;
        return new ChartOfAccountsCategoryTemplateReadDto
        {
            Id          = template.Id,
            Name        = template.Name ?? string.Empty,
            Description = template.Description,
            IsActive    = template.IsActive,
        };
    }

    public static ChartOfAccountsCategoryTemplate ToEntity(this CreateChartOfAccountsCategoryTemplateDto dto)
        => new ChartOfAccountsCategoryTemplate
        {
            Name        = dto.Name.Trim(),
            Description = dto.Description,
            IsActive    = dto.IsActive,
            IsDeleted   = false,
        };

    public static void ApplyUpdate(this ChartOfAccountsCategoryTemplate entity, UpdateChartOfAccountsCategoryTemplateDto dto)
    {
        entity.Name        = dto.Name.Trim();
        entity.Description = dto.Description;
        entity.IsActive    = dto.IsActive;
    }

    /// <summary>Crea una ChartOfAccountsCategory per il tenant specificato a partire da questo template.</summary>
    public static ChartOfAccountsCategory ToCategory(this ChartOfAccountsCategoryTemplate template, Tenant tenant)
        => new ChartOfAccountsCategory
        {
            Tenant      = tenant,
            Name        = template.Name,
            Description = template.Description,
            IsActive    = true,
            IsDeleted   = false,
        };
}
