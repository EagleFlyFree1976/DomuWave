namespace DomuWave.Services.Models;

public class AssemblyTypeLookup
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int Ordinaria    = 0;
    public const int Straordinaria = 1;

    public override int GetHashCode() => Id.GetHashCode();
}
