namespace DomuWave.Services.Models;

public class AssemblyTypeLookup
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int Ordinaria    = 1;
    public const int Straordinaria = 2;

    public override int GetHashCode() => Id.GetHashCode();
}
