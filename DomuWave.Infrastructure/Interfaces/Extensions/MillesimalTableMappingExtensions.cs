using CPQ.Core.Extensions;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class MillesimalTableMappingExtensions
{
    public static MillesimalTableReadDto ToReadDto(this MillesimalTable entity)
    {
        if (entity == null) return null;
        var dto = new MillesimalTableReadDto
        {
            CondominiumId   = entity.Condominium?.Id ?? 0,
            Code            = entity.Code ?? string.Empty,
            TotalMillesimal = entity.TotalMillesimal,
            IsActive        = entity.IsActive,
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
            TotalMillesimal = dto.TotalMillesimal,
            IsActive        = dto.IsActive,
        };
    }

    public static void ApplyUpdate(this MillesimalTable entity, UpdateMillesimalTableDto dto)
    {
        entity.Code            = dto.Code;
        entity.TotalMillesimal = dto.TotalMillesimal;
        entity.IsActive        = dto.IsActive;
    }
}
