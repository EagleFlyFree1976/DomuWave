namespace DomuWave.Services.Models;

public class AssemblyStatusLookup
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int Bozza       = 1;
    public const int Pianificata = 2;
    public const int Convocata   = 3;
    public const int Svolta      = 4;
    public const int Annullata   = 5;

    public override int GetHashCode() => Id.GetHashCode();
}
