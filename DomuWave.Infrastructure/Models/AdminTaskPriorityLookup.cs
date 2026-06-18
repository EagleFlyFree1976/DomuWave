namespace DomuWave.Services.Models
{
    /// <summary>
    /// Lookup per la priorità di un'attività (AdminTask). Valori fissi.
    /// </summary>
    public class AdminTaskPriorityLookup
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; } = string.Empty;

        public const int Bassa  = 1;
        public const int Media  = 2;
        public const int Alta   = 3;
        public const int Urgente = 4;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
