using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Staircase;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class StaircaseMappingExtensions
{
    public static StaircaseReadDto ToReadDto(this Staircase entity)
    {
        if (entity == null) return null;
        var dto = new StaircaseReadDto
        {
            CondominiumId   = entity.Condominium?.Id   ?? 0,
            CondominiumName = entity.Condominium?.Name,
            BuildingId      = entity.Building?.Id,
            BuildingName    = entity.Building?.Name,
            Name            = entity.Name,
            IsActive        = entity.IsActive,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Staircase ToEntity(this CreateStaircaseDto dto, Condominium condominium, Building? building, Tenant tenant)
    {
        if (dto == null) return null;
        return new Staircase
        {
            Condominium = condominium,
            Building    = building,
            Tenant      = tenant,
            Name        = dto.Name,
            IsActive    = dto.IsActive,
        };
    }

    public static void ApplyUpdate(this Staircase entity, UpdateStaircaseDto dto, Building? building)
    {
        entity.Building = building;
        entity.Name     = dto.Name;
        entity.IsActive = dto.IsActive;
    }
}
