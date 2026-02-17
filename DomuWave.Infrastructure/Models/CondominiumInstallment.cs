using System;
namespace DomuWave.Services.Models
{
    public class CondominiumInstallment
    {
        public virtual int InstallmentId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual int? BudgetId { get; set; }
        public virtual int Year { get; set; }
        public virtual int InstallmentNumber { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime DueDate { get; set; }
        public virtual decimal TotalAmount { get; set; }
        public virtual string Status { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public virtual Budget Budget { get; set; }
        public CondominiumInstallment()
        {
            Status = "Open";
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
