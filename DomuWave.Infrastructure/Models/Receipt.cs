using System;
namespace DomuWave.Services.Models
{
    public class Receipt
    {
        public virtual long ReceiptId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual long? FeeId { get; set; }
        public virtual long UserId { get; set; }
        public virtual int UnitId { get; set; }
        public virtual DateTime ReceiptDate { get; set; }
        public virtual DateTime RegistrationDate { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string Description { get; set; }
        public virtual string BankReference { get; set; }
        public virtual string ReconciliationStatus { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public virtual CondominiumFee CondominiumFee { get; set; }
        public virtual RealEstateUnit RealEstateUnit { get; set; }
        public Receipt()
        {
            RegistrationDate = DateTime.UtcNow;
            ReconciliationStatus = "ToReconcile";
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
