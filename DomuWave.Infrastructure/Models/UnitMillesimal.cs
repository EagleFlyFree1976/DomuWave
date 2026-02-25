using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class UnitMillesimal : TenantEntity<int>
    {
        public virtual MillesimalTable MillesimalTable { get; set; }
        public virtual RealEstateUnit Unit { get; set; }
        public virtual decimal Millesimal { get; set; }
        public virtual string Notes { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
