using System;
namespace DomuWave.Services.Models
{
    public class ExpenseAllocation
    {
        public virtual long ExpenseAllocationId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual long ExpenseId { get; set; }
        public virtual int UnitId { get; set; }
        public virtual decimal Millesimal { get; set; }
        public virtual decimal AllocatedAmount { get; set; }
        public virtual decimal AllocationPercentage { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Expense Expense { get; set; }
        public virtual RealEstateUnit RealEstateUnit { get; set; }
        public ExpenseAllocation()
        {
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
