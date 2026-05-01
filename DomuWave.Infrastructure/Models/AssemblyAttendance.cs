namespace DomuWave.Services.Models;

public class AssemblyAttendance : TenantEntity<int>
{
    // Name (base) → non usato significativamente, verrà impostato al nome del proprietario

    public virtual Assembly             Assembly        { get; set; }
    public virtual UnitOwner            UnitOwner       { get; set; }
    public virtual AttendanceTypeLookup AttendanceType  { get; set; }
    public virtual string?              DelegateName    { get; set; }
    public virtual decimal              MillesimalValue { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
