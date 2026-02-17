using System;
namespace DomuWave.Services.Models
{
    public class ChartOfAccounts
    {
        public virtual int AccountId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual string Type { get; set; }
        public virtual string Category { get; set; }
        public virtual int? ParentAccountId { get; set; }
        public virtual int Level { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsActive { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public virtual ChartOfAccounts ParentAccount { get; set; }
        public ChartOfAccounts()
        {
            Level = 1;
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
