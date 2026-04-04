using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Maintenance;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class MaintenanceMappingExtensions
{
    public static MaintenanceReadDto ToReadDto(this Maintenance entity)
    {
        if (entity == null) return null;

        var dto = new MaintenanceReadDto
        {
            TenantId        = entity.Tenant?.Id      ?? Guid.Empty,
            CondominiumId   = entity.Condominium?.Id ?? 0,
            CondominiumName = entity.Condominium?.Name,
            SupplierId      = entity.Supplier?.Id,
            SupplierName    = entity.Supplier?.CompanyName,
            Title           = entity.Title,
            Description     = entity.Description,
            Category        = entity.Category,
            Priority        = entity.Priority,
            Status          = entity.Status,
            ReportedDate    = entity.ReportedDate,
            ScheduledDate   = entity.ScheduledDate,
            CompletedDate   = entity.CompletedDate,
            EstimatedCost   = entity.EstimatedCost,
            ActualCost      = entity.ActualCost,
            Notes           = entity.Notes,
            DocumentPath    = entity.DocumentPath,
        };

        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Maintenance ToEntity(this CreateMaintenanceDto dto, Condominium condominium, Supplier supplier)
    {
        if (dto == null) return null;

        return new Maintenance
        {
            Tenant        = condominium.Tenant,
            Condominium   = condominium,
            Supplier      = supplier,
            Name          = dto.Title,
            Title         = dto.Title,
            Description   = dto.Description,
            Category      = dto.Category,
            Priority      = dto.Priority ?? "Medium",
            Status        = dto.Status ?? "Open",
            ReportedDate  = dto.ReportedDate,
            ScheduledDate = dto.ScheduledDate,
            CompletedDate = dto.CompletedDate,
            EstimatedCost = dto.EstimatedCost,
            ActualCost    = dto.ActualCost,
            Notes         = dto.Notes,
            DocumentPath  = dto.DocumentPath,
        };
    }

    public static void ApplyUpdate(this Maintenance entity, UpdateMaintenanceDto dto, Supplier supplier)
    {
        entity.Name          = dto.Title;
        entity.Supplier      = supplier;
        entity.Description   = dto.Description;
        entity.Category      = dto.Category;
        entity.Priority      = dto.Priority;
        entity.Status        = dto.Status;
        entity.ReportedDate  = dto.ReportedDate;
        entity.ScheduledDate = dto.ScheduledDate;
        entity.CompletedDate = dto.CompletedDate;
        entity.EstimatedCost = dto.EstimatedCost;
        entity.ActualCost    = dto.ActualCost;
        entity.Notes         = dto.Notes;
        entity.DocumentPath  = dto.DocumentPath;
    }
}
