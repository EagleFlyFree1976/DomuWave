using CPQ.Core.Extensions;
using DomuWave.Services.Dto.UnitMillesimal;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class UnitMillesimalMappingExtensions
{
    public static UnitMillesimalReadDto ToReadDto(this UnitMillesimal entity)
    {
        if (entity == null) return null;
        var dto = new UnitMillesimalReadDto
        {
            MillesimalTableId = entity.MillesimalTable?.Id ?? 0,
            UnitId            = entity.Unit?.Id ?? 0,
            UnitNumber        = entity.Unit?.InternalNumber ?? string.Empty,
            UnitFloor         = entity.Unit?.Floor ?? 0,
            UnitStaircase     = entity.Unit?.Staircase,
            Millesimal        = entity.Millesimal,
            Notes             = entity.Notes,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static UnitMillesimal ToEntity(this CreateUnitMillesimalDto dto, MillesimalTable table, RealEstateUnit unit)
    {
        return new UnitMillesimal
        {
            Tenant          = table.Tenant,
            MillesimalTable = table,
            Unit            = unit,
            Millesimal      = dto.Millesimal,
            Name            = dto.Notes,
        };
    }

    public static void ApplyUpdate(this UnitMillesimal entity, UpdateUnitMillesimalDto dto)
    {
        entity.Millesimal = dto.Millesimal;
        entity.Name       = dto.Notes;
    }
}
