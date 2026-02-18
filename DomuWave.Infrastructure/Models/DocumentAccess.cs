using System;
using DomuWave.Services.Models;

namespace DomuWave.Domain.Models
{
    public class DocumentAccess : TenantEntity<long>
    {
        public virtual Document Document { get; set; }
        public virtual long UserId { get; set; }
        public virtual DateTime AccessDate { get; set; }
        public virtual string AccessType { get; set; }
        public virtual string IpAddress { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
