using System;
namespace DomuWave.Services.Models
{
    public class BudgetItem : TenantEntity<int>
    {
        
       
        public virtual int BudgetId { get; set; }
        public virtual int AccountId { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Description { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Budget Budget { get; set; }
        public virtual ChartOfAccounts Account { get; set; }
        public BudgetItem()
        {
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
