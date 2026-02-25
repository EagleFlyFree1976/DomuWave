using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class CommunicationRead : TenantEntity<int>
    {
        public virtual Communication Communication { get; set; }
        public virtual long UserId { get; set; }
        public virtual DateTime ReadDate { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
