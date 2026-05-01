namespace DomuWave.Services.Models;

public class AttendanceTypeLookup
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int Presente  = 0;
    public const int Delegato  = 1;
    public const int Assente   = 2;

    public override int GetHashCode() => Id.GetHashCode();
}
