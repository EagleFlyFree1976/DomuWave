using CPQ.Core;

namespace DomuWave.Services.Models.Import;

public class ImportMapping : TraceEntity<long>
{
    public virtual Import Import { get; set; }

    public virtual string Entity { get; set; }
    public virtual string SourceValue { get; set; }
    public virtual string TargetValue { get; set; }
    public virtual string TrasformationRule { get; set; }

    public virtual bool IsActive { get; set; }

    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}