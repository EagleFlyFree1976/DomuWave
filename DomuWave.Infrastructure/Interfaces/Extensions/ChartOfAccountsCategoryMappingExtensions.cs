using DomuWave.Services.Dto.ChartOfAccountsCategory;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ChartOfAccountsCategoryMappingExtensions
{
    public static ChartOfAccountsCategoryReadDto ToReadDto(this ChartOfAccountsCategory category)
    {
        if (category == null) return null;
        return new ChartOfAccountsCategoryReadDto
        {
            Id          = category.Id,
            Name        = category.Name ?? string.Empty,
            Description = category.Description,
            IsActive    = category.IsActive,
        };
    }

    public static ChartOfAccountsCategory ToEntity(this CreateChartOfAccountsCategoryDto dto, Tenant tenant)
        => new ChartOfAccountsCategory
        {
            Tenant      = tenant,
            Name        = dto.Name.Trim(),
            Description = dto.Description,
            IsActive    = dto.IsActive,
            IsDeleted   = false,
        };

    public static void ApplyUpdate(this ChartOfAccountsCategory entity, UpdateChartOfAccountsCategoryDto dto)
    {
        entity.Name        = dto.Name.Trim();
        entity.Description = dto.Description;
        entity.IsActive    = dto.IsActive;
    }
}
