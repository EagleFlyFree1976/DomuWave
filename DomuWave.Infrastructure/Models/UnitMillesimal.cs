using System;
namespace DomuWave.Services.Models
{
    public class UnitMillesimal
    {
        public virtual int UnitMillesimalId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int MillesimalTableId { get; set; }
        public virtual int UnitId { get; set; }
        public virtual decimal Millesimal { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual MillesimalTable MillesimalTable { get; set; }
        public virtual RealEstateUnit RealEstateUnit { get; set; }
        public UnitMillesimal()
        {
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
