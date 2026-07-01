namespace DomuWave.Services.Models
{
    /// <summary>
    /// Opzione di risposta di un sondaggio in bacheca (<see cref="BoardPost"/> con IsPoll=true).
    /// </summary>
    public class BoardPostOption : TenantEntity<int>
    {
        public virtual BoardPost Post { get; set; } = null!;

        // Testo dell'opzione. Mappato sulla colonna "Text" (Name dalla base TenantEntity).
        // L'ordine di visualizzazione è dato da OrderKey.
        public virtual int OrderKey { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
