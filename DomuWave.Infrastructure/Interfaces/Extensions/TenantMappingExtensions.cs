using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

/// <summary>
/// Estensioni di mapping per il dominio Tenant.
/// </summary>
public static class TenantMappingExtensions
{
    /// <summary>
    /// Proietta un'entità <see cref="Tenant"/> nel suo DTO di lettura.
    /// </summary>
    public static TenantReadDto ToReadDto(this Tenant tenant)
    {
        if (tenant == null) return null;

        var dto = new TenantReadDto
        {
            Id   = tenant.Id,
            Name = tenant.Name,
            Code = tenant.Code,
            IsActive = tenant.IsActive,
        };

        // Audit fields (da TraceEntityDTO tramite DtoExtensions)
        dto.fillTraceData(tenant);

        return dto;
    }
}
