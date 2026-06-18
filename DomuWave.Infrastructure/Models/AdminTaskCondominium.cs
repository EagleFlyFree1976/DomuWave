namespace DomuWave.Services.Models
{
    /// <summary>
    /// Collegamento tra un'attività (AdminTask) e un condominio.
    /// Un task può essere agganciato a 0, 1 o N condomìni.
    /// </summary>
    public class AdminTaskCondominium : TenantEntity<int>
    {
        public virtual AdminTask   Task        { get; set; } = null!;
        public virtual Condominium Condominium { get; set; } = null!;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
