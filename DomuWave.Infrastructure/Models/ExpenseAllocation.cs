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
        /// <summary>Arrotondamento applicato a questa unità per pareggiare il totale (0 per quasi tutte le unità).</summary>
        public virtual decimal RoundingAdjustment { get; set; } = 0;
        public virtual string Notes { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
