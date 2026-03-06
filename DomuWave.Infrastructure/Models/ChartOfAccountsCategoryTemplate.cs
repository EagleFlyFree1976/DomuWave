namespace DomuWave.Services.Models;

/// <summary>
/// Template di categoria del piano dei conti.
/// Usato per popolare automaticamente le categorie di un nuovo tenant.
/// Non ha TenantId: è una tabella di sistema condivisa.
/// </summary>
public class ChartOfAccountsCategoryTemplate : GenericEntity<int>
{
    public virtual bool IsActive { get; set; } = true;

    public override int GetHashCode() => Id.GetHashCode();
}
