using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class ExpenseAllocation : TenantEntity<long>
    {
        public virtual Expense Expense { get; set; }
        public virtual RealEstateUnit Unit { get; set; }
        public virtual decimal Millesimal { get; set; }
        public virtual decimal AllocatedAmount { get; set; }
        public virtual decimal AllocationPercentage { get; set; }
        public virtual string Notes { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
