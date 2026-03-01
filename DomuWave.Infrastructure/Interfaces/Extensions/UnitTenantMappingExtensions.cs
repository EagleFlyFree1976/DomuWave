using CPQ.Core.Extensions;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class UnitTenantMappingExtensions
{
    public static UnitTenantReadDto ToReadDto(this UnitTenant tenant)
    {
        if (tenant == null) return null;

        var dto = new UnitTenantReadDto
        {
            UnitId         = tenant.Unit?.Id ?? 0,
            UserId         = tenant.UserId,
            FirstName      = tenant.FirstName,
            LastName       = tenant.LastName,
            TaxCode        = tenant.TaxCode,
            Email          = tenant.Email,
            Phone          = tenant.Phone,
            LeaseStartDate = tenant.LeaseStartDate,
            LeaseEndDate   = tenant.LeaseEndDate,
            ExpensePayer   = tenant.ExpensePayer,
            IsActive       = tenant.IsActive,
            Notes          = tenant.Notes,
        };

        dto.SetTraceInfo(tenant);
        return dto;
    }

    public static UnitTenant ToEntity(this CreateUnitTenantDto dto, RealEstateUnit unit, Tenant tenant)
    {
        if (dto == null) return null;

        return new UnitTenant
        {
            Unit           = unit,
            Tenant         = tenant,
            UserId         = dto.UserId,
            Name           = dto.LastName ?? string.Empty,
            FirstName      = dto.FirstName ?? string.Empty,
            TaxCode        = dto.TaxCode,
            Email          = dto.Email,
            Phone          = dto.Phone,
            LeaseStartDate = dto.LeaseStartDate,
            LeaseEndDate   = dto.LeaseEndDate,
            ExpensePayer   = dto.ExpensePayer ?? string.Empty,
            IsActive       = dto.IsActive,
            Description    = dto.Notes,
        };
    }

    public static void ApplyUpdate(this UnitTenant entity, UpdateUnitTenantDto dto)
    {
        entity.FirstName      = dto.FirstName ?? string.Empty;
        entity.Name           = dto.LastName ?? string.Empty;
        entity.Email          = dto.Email;
        entity.Phone          = dto.Phone;
        entity.LeaseStartDate = dto.LeaseStartDate;
        entity.LeaseEndDate   = dto.LeaseEndDate;
        entity.TaxCode        = dto.TaxCode;
        entity.ExpensePayer   = dto.ExpensePayer ?? string.Empty;
        entity.IsActive       = dto.IsActive;
        entity.Description    = dto.Notes;
    }
}
