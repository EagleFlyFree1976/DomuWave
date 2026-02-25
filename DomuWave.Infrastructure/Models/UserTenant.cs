using DomuWave.Services.Models;

namespace DomuWave.Services.Models;

/// <summary>
/// Associazione tra un utente del sistema e un tenant (azienda/studio).
/// Un utente può appartenere a più tenant, ma uno solo è marcato come default.
/// </summary>
public class UserTenant : TenantEntity<int>
{
    /// <summary>ID dell'utente nel sistema di autenticazione.</summary>
    public virtual long UserId { get; set; }

    /// <summary>Tenant associato.</summary>
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Indica se questo è il tenant di default per l'utente.
    /// Solo uno per UserId può avere questo flag a true.
    /// </summary>
    public virtual bool IsDefault { get; set; }

    /// <summary>Indica se l'associazione è attiva.</summary>
    public virtual bool IsActive { get; set; } = true;

    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}