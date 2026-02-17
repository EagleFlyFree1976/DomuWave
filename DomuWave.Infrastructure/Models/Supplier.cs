using System;
namespace DomuWave.Services.Models
{
    public class Supplier
    {
        public virtual int SupplierId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string VatNumber { get; set; }
        public virtual string TaxCode { get; set; }
        public virtual string Address { get; set; }
        public virtual string City { get; set; }
        public virtual string Province { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Pec { get; set; }
        public virtual string ContactPerson { get; set; }
        public virtual string SupplierType { get; set; }
        public virtual string PaymentTerms { get; set; }
        public virtual string IbanAccount { get; set; }
        public virtual string Notes { get; set; }
        public virtual bool IsActive { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public Supplier()
        {
            IsActive = true;
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
