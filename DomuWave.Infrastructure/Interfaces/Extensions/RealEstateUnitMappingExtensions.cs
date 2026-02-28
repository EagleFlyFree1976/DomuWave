using CPQ.Core.Extensions;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

/// <summary>
/// Estensioni di mapping per il dominio RealEstateUnit.
/// </summary>
public static class RealEstateUnitMappingExtensions
{
    /// <summary>
    /// Proietta un'entità <see cref="RealEstateUnit"/> nel suo DTO di lettura.
    /// </summary>
    public static RealEstateUnitReadDto ToReadDto(this RealEstateUnit unit)
    {
        if (unit == null) return null;

        var dto = new RealEstateUnitReadDto
        {
            CondominiumId   = unit.Condominium?.Id ?? 0,
            CondominiumName = unit.Condominium?.Name,
            Staircase       = unit.Staircase,
            Floor           = unit.Floor,
            InternalNumber  = unit.InternalNumber,
            Subordinate     = unit.Subordinate,
            Category        = unit.Category,
            CadastralIncome = unit.CadastralIncome,
            AreaSqm         = unit.AreaSqm,
            Rooms           = unit.Rooms,
            UnitType        = unit.UnitType,
            OccupancyStatus = unit.OccupancyStatus,
            Notes           = unit.Notes,
            IsActive        = unit.IsActive,
        };

        dto.SetTraceInfo(unit);
        return dto;
    }

    /// <summary>
    /// Crea una nuova entità <see cref="RealEstateUnit"/> a partire dal DTO di creazione.
    /// </summary>
    public static RealEstateUnit ToEntity(this CreateRealEstateUnitDto dto, Condominium condominium, Tenant tenant)
    {
        if (dto == null) return null;

        return new RealEstateUnit
        {
            Condominium     = condominium,
            Tenant          = tenant,
            Staircase       = dto.Staircase,
            Floor           = dto.Floor,
            InternalNumber  = dto.InternalNumber,
            Subordinate     = dto.Subordinate,
            Category        = dto.Category,
            CadastralIncome = dto.CadastralIncome,
            AreaSqm         = dto.AreaSqm,
            Rooms           = dto.Rooms,
            UnitType        = dto.UnitType ?? string.Empty,
            OccupancyStatus = dto.OccupancyStatus ?? string.Empty,
            Notes           = dto.Notes,
            IsActive        = dto.IsActive,
        };
    }

    /// <summary>
    /// Applica i campi del DTO di aggiornamento all'entità <see cref="RealEstateUnit"/> esistente.
    /// </summary>
    public static void ApplyUpdate(this RealEstateUnit entity, UpdateRealEstateUnitDto dto)
    {
        entity.Staircase       = dto.Staircase;
        entity.Floor           = dto.Floor;
        entity.InternalNumber  = dto.InternalNumber;
        entity.Subordinate     = dto.Subordinate;
        entity.Category        = dto.Category;
        entity.CadastralIncome = dto.CadastralIncome;
        entity.AreaSqm         = dto.AreaSqm;
        entity.Rooms           = dto.Rooms;
        entity.UnitType        = dto.UnitType ?? string.Empty;
        entity.OccupancyStatus = dto.OccupancyStatus ?? string.Empty;
        entity.Notes           = dto.Notes;
        entity.IsActive        = dto.IsActive;
    }
}
