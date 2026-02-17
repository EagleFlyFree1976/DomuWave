using System;
namespace DomuWave.Services.Models
{
    public class CondominiumFee
    {
        public virtual long FeeId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int InstallmentId { get; set; }
        public virtual int UnitId { get; set; }
        public virtual long UserId { get; set; }
        public virtual decimal AmountDue { get; set; }
        public virtual decimal AmountPaid { get; set; }
        public virtual decimal Balance { get; set; }
        public virtual string PaymentStatus { get; set; }
        public virtual DateTime? PaymentDate { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual CondominiumInstallment CondominiumInstallment { get; set; }
        public virtual RealEstateUnit RealEstateUnit { get; set; }
        public CondominiumFee()
        {
            AmountPaid = 0;
            PaymentStatus = "ToPay";
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
