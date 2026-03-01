using CPQ.Core.Extensions;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class UnitOwnerMappingExtensions
{
    public static UnitOwnerReadDto ToReadDto(this UnitOwner owner)
    {
        if (owner == null) return null;

        var dto = new UnitOwnerReadDto
        {
            UnitId         = owner.Unit?.Id ?? 0,
            UserId         = owner.UserId,
            OwnerType      = owner.OwnerType,
            OwnershipQuota = owner.OwnershipQuota,
            StartDate      = owner.StartDate,
            EndDate        = owner.EndDate,
            IsResident     = owner.IsResident,
            IsActive       = owner.IsActive,
            Notes          = owner.Notes,
        };

        dto.SetTraceInfo(owner);
        return dto;
    }

    public static UnitOwner ToEntity(this CreateUnitOwnerDto dto, RealEstateUnit unit, Tenant tenant)
    {
        if (dto == null) return null;

        return new UnitOwner
        {
            Unit           = unit,
            Tenant         = tenant,
            Name           = dto.OwnerType ?? string.Empty,
            UserId         = dto.UserId,
            OwnershipQuota = dto.OwnershipQuota,
            StartDate      = dto.StartDate,
            EndDate        = dto.EndDate,
            IsResident     = dto.IsResident,
            IsActive       = dto.IsActive,
            Description    = dto.Notes,
        };
    }

    public static UserUnitOwnerDto ToUserUnitOwnerDto(this UnitOwner owner)
    {
        if (owner == null) return null;

        return new UserUnitOwnerDto
        {
            UnitId          = owner.Unit?.Id ?? 0,
            CondominiumId   = owner.Unit?.Condominium?.Id ?? 0,
            CondominiumName = owner.Unit?.Condominium?.Name,
            TenantId        = owner.Tenant?.Id.ToString() ?? string.Empty,
        };
    }

    public static void ApplyUpdate(this UnitOwner entity, UpdateUnitOwnerDto dto)
    {
        entity.Name           = dto.OwnerType ?? string.Empty;
        entity.OwnershipQuota = dto.OwnershipQuota;
        entity.StartDate      = dto.StartDate;
        entity.EndDate        = dto.EndDate;
        entity.IsResident     = dto.IsResident;
        entity.IsActive       = dto.IsActive;
        entity.Description    = dto.Notes;
    }
}
