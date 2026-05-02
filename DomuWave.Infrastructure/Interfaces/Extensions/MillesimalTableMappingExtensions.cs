using CPQ.Core.Extensions;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class MillesimalTableMappingExtensions
{
    public static MillesimalTableReadDto ToReadDto(this MillesimalTable entity, decimal calculatedMillesimal = 0m)
    {
        if (entity == null) return null;
        var dto = new MillesimalTableReadDto
        {
            TenantId             = entity.Tenant?.Id      ?? Guid.Empty,
            CondominiumId        = entity.Condominium?.Id ?? 0,
            Code                 = entity.Code ?? string.Empty,
            Name                 = entity.Name,
            Description          = entity.Description,
            TotalMillesimal      = entity.TotalMillesimal,
            IsActive             = entity.IsActive,
            IsDraft              = entity.IsDraft,
            IsEnabled            = entity.IsEnabled,
            IsDefault            = entity.IsDefault,
            CalculatedMillesimal = calculatedMillesimal,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static MillesimalTable ToEntity(this CreateMillesimalTableDto dto, Condominium condominium)
    {
        return new MillesimalTable
        {
            Tenant          = condominium.Tenant,
            Condominium     = condominium,
            Code            = dto.Code,
            Name            = dto.Name ?? dto.Code,
            Description     = dto.Description,
            TotalMillesimal = dto.TotalMillesimal,
            IsActive        = false,
            IsDraft         = true,
            IsEnabled       = true,
            IsDefault       = false,
        };
    }

    public static void ApplyUpdate(this MillesimalTable entity, UpdateMillesimalTableDto dto)
    {
        entity.Code            = dto.Code;
        entity.Name            = dto.Name ?? dto.Code;
        entity.Description     = dto.Description;
        entity.TotalMillesimal = dto.TotalMillesimal;
        // IsActive / IsDraft are managed automatically based on millesimal sum
    }
}
