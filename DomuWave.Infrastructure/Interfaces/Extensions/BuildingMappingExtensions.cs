using CPQ.Core.Extensions;
using DomuWave.Services.Dto.Building;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class BuildingMappingExtensions
{
    public static BuildingReadDto ToReadDto(this Building entity)
    {
        if (entity == null) return null;
        var dto = new BuildingReadDto
        {
            CondominiumId   = entity.Condominium?.Id   ?? 0,
            CondominiumName = entity.Condominium?.Name,
            Name = entity.Name,
            Code            = entity.Code,
            Address         = entity.Address,
            YearOfConstruction = entity.YearOfConstruction,
            NumberOfFloors  = entity.NumberOfFloors,
            HasElevator     = entity.HasElevator,
            IsActive        = entity.IsActive,
            UnitCount       = entity.Units?.Count ?? 0,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static Building ToEntity(this CreateBuildingDto dto, Condominium condominium, Tenant tenant)
    {
        if (dto == null) return null;
        return new Building
        {
            Condominium        = condominium,
            Tenant             = tenant,
            Name               = dto.Name,
            Description        = dto.Description,
            Code               = dto.Code,
            Address            = dto.Address,
            YearOfConstruction = dto.YearOfConstruction,
            NumberOfFloors     = dto.NumberOfFloors,
            HasElevator        = dto.HasElevator,
            IsActive           = dto.IsActive,
        };
    }

    public static void ApplyUpdate(this Building entity, UpdateBuildingDto dto)
    {
        entity.Name               = dto.Name;
        entity.Description        = dto.Description;
        entity.Code               = dto.Code;
        entity.Address            = dto.Address;
        entity.YearOfConstruction = dto.YearOfConstruction;
        entity.NumberOfFloors     = dto.NumberOfFloors;
        entity.HasElevator        = dto.HasElevator;
        entity.IsActive           = dto.IsActive;
    }
}
