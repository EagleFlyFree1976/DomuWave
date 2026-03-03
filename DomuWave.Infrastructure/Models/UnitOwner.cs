using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class UnitOwner : TenantEntity<int>
    {
        public virtual RealEstateUnit Unit { get; set; }
        public virtual long UserId { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
        public virtual string OwnerType { get; set; }
        public virtual decimal OwnershipQuota { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual bool IsResident { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsAccessEnabled { get; set; }
        public virtual string Notes { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
