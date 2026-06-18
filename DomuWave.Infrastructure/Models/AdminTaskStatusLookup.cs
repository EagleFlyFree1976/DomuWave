namespace DomuWave.Services.Models
{
    /// <summary>
    /// Lookup per lo stato di un'attività (AdminTask). Valori fissi.
    /// </summary>
    public class AdminTaskStatusLookup
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; } = string.Empty;

        public const int DaFare     = 1;
        public const int InCorso    = 2;
        public const int Completata = 3;
        public const int Annullata  = 4;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
