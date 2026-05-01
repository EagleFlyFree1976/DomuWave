namespace DomuWave.Services.Models;

public class Assembly : TenantEntity<int>
{
    // Name (base) → Title dell'assemblea
    // Description (base) → Note brevi

    public virtual Condominium           Condominium   { get; set; }
    public virtual FiscalYear?           FiscalYear    { get; set; }
    public virtual AssemblyTypeLookup    AssemblyType  { get; set; }
    public virtual AssemblyStatusLookup  Status        { get; set; }
    public virtual DateTime              ScheduledDate { get; set; }
    public virtual DateTime?             ActualDate    { get; set; }
    public virtual string?               Location      { get; set; }
    public virtual string?               Notes         { get; set; }
    public virtual string?               MinutesPath   { get; set; }

    public virtual IList<AssemblyAgendaItem> AgendaItems { get; set; } = new List<AssemblyAgendaItem>();
    public virtual IList<AssemblyAttendance> Attendances { get; set; } = new List<AssemblyAttendance>();

    public override int GetHashCode() => Id.GetHashCode();
}
