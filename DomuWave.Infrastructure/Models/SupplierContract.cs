using System;
namespace DomuWave.Services.Models
{
    public class SupplierContract
    {
        public virtual int ContractId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual int SupplierId { get; set; }
        public virtual string ContractNumber { get; set; }
        public virtual string Subject { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual decimal? AnnualAmount { get; set; }
        public virtual string Frequency { get; set; }
        public virtual bool AutoRenewal { get; set; }
        public virtual string Status { get; set; }
        public virtual string DocumentPath { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public virtual Supplier Supplier { get; set; }
        public SupplierContract()
        {
            AutoRenewal = false;
            Status = "Active";
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
