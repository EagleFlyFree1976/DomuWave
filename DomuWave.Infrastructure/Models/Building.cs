using System.Collections.Generic;

namespace DomuWave.Services.Models
{
    public class Building : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Code { get; set; }
        public virtual string Address { get; set; }
        public virtual int? YearOfConstruction { get; set; }
        public virtual int? NumberOfFloors { get; set; }
        public virtual bool HasElevator { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual IList<RealEstateUnit> Units { get; set; } = new List<RealEstateUnit>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}
