using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class UnitTenant : TenantEntity<int>
    {
        public virtual RealEstateUnit Unit { get; set; }
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

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
