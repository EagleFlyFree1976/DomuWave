using System;
using CPQ.Core;

namespace DomuWave.Services.Models
{
    public class Tenant : TraceEntity<Guid>
    {
  
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual bool IsActive { get; set; }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }
    }
}
