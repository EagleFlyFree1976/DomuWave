using System;
namespace DomuWave.Services.Models
{
    public class Budget
    {
        public virtual int BudgetId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual int Year { get; set; }
        public virtual string Type { get; set; }
        public virtual DateTime? ApprovalDate { get; set; }
        public virtual string Status { get; set; }
        public virtual decimal TotalIncome { get; set; }
        public virtual decimal TotalExpenses { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public Budget()
        {
            Status = "Draft";
            TotalIncome = 0;
            TotalExpenses = 0;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
