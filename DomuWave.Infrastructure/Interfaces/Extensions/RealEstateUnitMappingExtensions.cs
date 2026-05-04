using CPQ.Core.Extensions;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Models;
using System.Linq;

namespace DomuWave.Services.Interfaces.Extensions;

/// <summary>
/// Estensioni di mapping per il dominio RealEstateUnit.
/// </summary>
public static class RealEstateUnitMappingExtensions
{
    /// <summary>
    /// Formato univoco del nome unità: "{InternalNumber} - {DisplayName}".
    /// Se DisplayName è vuoto restituisce solo InternalNumber.
    /// </summary>
    public static string FormatUnitName(string internalNumber, string displayName)
        => !string.IsNullOrWhiteSpace(displayName)
            ? $"{internalNumber} - {displayName}"
            : internalNumber ?? string.Empty;

    /// <summary>Overload su entità.</summary>
    public static string FormatUnitName(this RealEstateUnit unit)
        => FormatUnitName(unit?.InternalNumber, unit?.DisplayName);

    /// <summary>
    /// Proietta un'entità <see cref="RealEstateUnit"/> nel suo DTO di lettura.
    /// </summary>
    public static RealEstateUnitReadDto ToReadDto(this RealEstateUnit unit)
    {
        if (unit == null) return null;

        var dto = new RealEstateUnitReadDto
        {
            TenantId        = unit.Tenant?.Id      ?? Guid.Empty,
            CondominiumId   = unit.Condominium?.Id ?? 0,
            CondominiumName = unit.Condominium?.Name,
            BuildingId      = unit.Building?.Id,
            BuildingName    = unit.Building?.Name,
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
            DisplayName     = unit.DisplayName,
            NumeroAbitanti  = unit.NumeroAbitanti,
            IsActive        = unit.IsActive,
        };

        dto.SetTraceInfo(unit);
        return dto;
    }

    public static RealEstateUnitPanoramaDto ToPanoramaDto(
        this RealEstateUnit unit,
        IList<UnitOwnerReadDto>  owners,
        IList<UnitTenantReadDto> tenants)
    {
        if (unit == null) return null;
        var dto = new RealEstateUnitPanoramaDto
        {
            TenantId        = unit.Tenant?.Id      ?? Guid.Empty,
            CondominiumId   = unit.Condominium?.Id ?? 0,
            CondominiumName = unit.Condominium?.Name,
            BuildingId      = unit.Building?.Id,
            BuildingName    = unit.Building?.Name,
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
            DisplayName     = unit.DisplayName,
            NumeroAbitanti  = unit.NumeroAbitanti,
            IsActive        = unit.IsActive,
            Owners          = owners,
            Tenants         = tenants,
        };
        dto.SetTraceInfo(unit);
        return dto;
    }

    /// <summary>
    /// Crea una nuova entità <see cref="RealEstateUnit"/> a partire dal DTO di creazione.
    /// </summary>
    public static RealEstateUnit ToEntity(this CreateRealEstateUnitDto dto, Condominium condominium, Tenant tenant, Building building = null)
    {
        if (dto == null) return null;

        return new RealEstateUnit
        {
            Condominium     = condominium,
            Tenant          = tenant,
            Building        = building,
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
            NumeroAbitanti  = dto.NumeroAbitanti,
            IsActive        = dto.IsActive,
        };
    }

    /// <summary>
    /// Ricalcola il DisplayName dell'unità in base ai cognomi dei proprietari attivi.
    /// Se <paramref name="overrideName"/> è valorizzato, usa quello invece.
    /// </summary>
    public static void RefreshDisplayName(this RealEstateUnit unit, string overrideName = null)
    {
        if (!string.IsNullOrWhiteSpace(overrideName))
        {
            unit.DisplayName = overrideName.Trim();
            return;
        }

        var activeOwners = unit.Owners?
            .Where(o => o.IsActive && !o.IsDeleted)
            .OrderBy(o => o.LastName)
            .ToList();

        if (activeOwners == null || !activeOwners.Any())
            return; // lascia il DisplayName esistente invariato

        unit.DisplayName = string.Join(" / ",
            activeOwners.Select(o => string.IsNullOrWhiteSpace(o.LastName)
                ? o.FirstName
                : o.LastName).Where(n => !string.IsNullOrWhiteSpace(n)));
    }

    /// <summary>
    /// Applica i campi del DTO di aggiornamento all'entità <see cref="RealEstateUnit"/> esistente.
    /// </summary>
    public static void ApplyUpdate(this RealEstateUnit entity, UpdateRealEstateUnitDto dto, Building building = null)
    {
        entity.Building        = building;
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
        entity.DisplayName     = dto.DisplayName;
        entity.NumeroAbitanti  = dto.NumeroAbitanti;
        entity.IsActive        = dto.IsActive;
    }
}
