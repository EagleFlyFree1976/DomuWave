using CPQ.Core;

namespace DomuWave.Services.Models;

public abstract class GenericEntity<T> : TraceEntity<T>
{
    public virtual string Name { get; set; }
    public virtual string Description { get; set; }
}