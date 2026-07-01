using DomuWave.Services.Models;

namespace DomuWave.Services.Models;

/// <summary>
/// Associazione tra un utente del sistema e un tenant (azienda/studio).
/// Un utente pu� appartenere a pi� tenant, ma uno solo � marcato come default.
/// </summary>
public class UserTenant : TenantEntity<int>
{
    /// <summary>ID dell'utente nel sistema di autenticazione.</summary>
    public virtual int UserId { get; set; }

    /// <summary>Tenant associato.</summary>
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Indica se questo � il tenant di default per l'utente.
    /// Solo uno per UserId pu� avere questo flag a true.
    /// </summary>
    public virtual bool IsDefault { get; set; }

    /// <summary>Indica se l'associazione � attiva.</summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// Ruolo dell'utente IN QUESTO tenant: "Admin" | "Condomino".
    /// Null = legacy, fallback al ruolo globale dell'utente auth.
    /// Permette allo stesso utente di essere admin in un tenant e cond�mino in un altro.
    /// </summary>
    public virtual string? RoleCode { get; set; }

    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}