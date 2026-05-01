namespace DomuWave.Services.Models;

public class AssemblyStatusLookup
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int Convocata  = 0;
    public const int Svolta     = 1;
    public const int Annullata  = 2;

    public override int GetHashCode() => Id.GetHashCode();
}
