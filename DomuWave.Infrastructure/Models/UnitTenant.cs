using System;

namespace DomuWave.Services.Models
{
    public class UnitTenant
    {
        public virtual int TenantId { get; set; }
        public virtual Guid OwnerTenantId { get; set; }
        public virtual int UnitId { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string TaxCode { get; set; }
        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
        public virtual DateTime LeaseStartDate { get; set; }
        public virtual DateTime? LeaseEndDate { get; set; }
        public virtual string ContractPath { get; set; }
        public virtual string ExpensePayer { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        

        public virtual Tenant OwnerTenant { get; set; }
        public virtual RealEstateUnit RealEstateUnit { get; set; }

        public UnitTenant()
        {
            ExpensePayer = "Owner";
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
