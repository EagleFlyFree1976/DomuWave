using DomuWave.Services.Dto.Condominium;

namespace DomuWave.Services.Dto.UserTenants;

/// <summary>
/// Profilo dell'utente in uno specifico tenant. Usato dal frontend al cambio
/// tenant per ricalcolare profilo/menu/permessi (caso "ruolo diverso per tenant").
/// </summary>
public class TenantProfileDto
{
    /// <summary>1=SuperAdmin, 2=TenantAdministrator, 3=User/Condomino.</summary>
    public int Profile { get; set; }

    /// <summary>"Admin" | "Condomino" (da UserTenant.RoleCode).</summary>
    public string? RoleCode { get; set; }

    /// <summary>Se condòmino in questo tenant: i suoi condomìni nel tenant.</summary>
    public IList<CondominiumSummaryDto> Condominiums { get; set; } = new List<CondominiumSummaryDto>();
}
