using System;

namespace DomuWave.Services.Models
{
    public class UnitOwner
    {
        public virtual int UnitOwnerId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int UnitId { get; set; }
        public virtual long UserId { get; set; }
        public virtual string OwnerType { get; set; }
        public virtual decimal OwnershipQuota { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual bool IsResident { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        

        public virtual Tenant Tenant { get; set; }
        public virtual RealEstateUnit RealEstateUnit { get; set; }

        public UnitOwner()
        {
            OwnerType = "Owner";
            OwnershipQuota = 100.00M;
            IsResident = true;
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
