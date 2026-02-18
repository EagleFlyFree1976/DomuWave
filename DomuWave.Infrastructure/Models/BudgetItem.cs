using System;
using DomuWave.Services.Models;

namespace DomuWave.Domain.Models
{
    public class BudgetItem : TenantEntity<int>
    {
        public virtual Budget Budget { get; set; }
        public virtual ChartOfAccounts Account { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Notes { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
