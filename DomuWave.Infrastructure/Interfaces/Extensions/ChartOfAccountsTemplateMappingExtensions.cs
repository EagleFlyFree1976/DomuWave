using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class ChartOfAccountsTemplateMappingExtensions
{
    // ── Template ──────────────────────────────────────────────────────────────

    public static ChartOfAccountsTemplateReadDto ToReadDto(this ChartOfAccountsTemplate t) =>
        new()
        {
            Id          = t.Id,
            Name        = t.Name ?? string.Empty,
            Description = t.Description,
            IsActive    = t.IsActive,
            ItemCount   = t.Items?.Count(i => !i.IsDeleted) ?? 0,
        };

    public static ChartOfAccountsTemplate ToEntity(this CreateChartOfAccountsTemplateDto dto, Tenant tenant) =>
        new()
        {
            Tenant      = tenant,
            Name        = dto.Name.Trim(),
            Description = dto.Description,
            IsActive    = dto.IsActive,
            IsDeleted   = false,
        };

    public static void ApplyUpdate(this ChartOfAccountsTemplate entity, UpdateChartOfAccountsTemplateDto dto)
    {
        entity.Name        = dto.Name.Trim();
        entity.Description = dto.Description;
        entity.IsActive    = dto.IsActive;
    }

    // ── TemplateItem ──────────────────────────────────────────────────────────

    public static ChartOfAccountsTemplateItemReadDto ToReadDto(this ChartOfAccountsTemplateItem i) =>
        new()
        {
            Id           = i.Id,
            TemplateId   = i.Template?.Id ?? 0,
            ParentItemId = i.ParentItem?.Id,
            Code         = i.Code ?? string.Empty,
            Name         = i.Name ?? string.Empty,
            Description  = i.Description,
            Type         = i.Type,
            TypeLabel    = i.Type switch
            {
                ChartOfAccountsType.Entrata     => "Entrata",
                ChartOfAccountsType.Patrimoniale => "Patrimoniale",
                _                               => "Uscita",
            },
            SortOrder = i.SortOrder,
            IsActive  = i.IsActive,
        };

    public static ChartOfAccountsTemplateItem ToEntity(
        this SaveChartOfAccountsTemplateItemDto dto,
        ChartOfAccountsTemplate template,
        ChartOfAccountsTemplateItem? parent,
        Tenant tenant) =>
        new()
        {
            Template    = template,
            ParentItem  = parent,
            Tenant      = tenant,
            Code        = dto.Code.Trim(),
            Name        = dto.Name.Trim(),
            Description = dto.Description,
            Type        = dto.Type,
            SortOrder   = dto.SortOrder,
            IsActive    = dto.IsActive,
            IsDeleted   = false,
        };

    public static void ApplyUpdate(this ChartOfAccountsTemplateItem entity, SaveChartOfAccountsTemplateItemDto dto,
        ChartOfAccountsTemplateItem? parent)
    {
        entity.ParentItem  = parent;
        entity.Code        = dto.Code.Trim();
        entity.Name        = dto.Name.Trim();
        entity.Description = dto.Description;
        entity.Type        = dto.Type;
        entity.SortOrder   = dto.SortOrder;
        entity.IsActive    = dto.IsActive;
    }
}
