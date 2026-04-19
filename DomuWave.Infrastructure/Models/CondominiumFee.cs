using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class CondominiumFee : TenantEntity<long>
    {
        public virtual CondominiumInstallment Installment { get; set; }
        public virtual RealEstateUnit Unit { get; set; }
        public virtual long UserId { get; set; }
        public virtual decimal AmountDue { get; set; }
        public virtual decimal AmountPaid { get; set; }
        public virtual decimal Balance { get; set; }
        public virtual string PaymentStatus { get; set; }
        public virtual DateTime? PaymentDate { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string Notes        { get; set; }
        public virtual string PaymentCode  { get; set; } = string.Empty;

        public virtual IList<Receipt> Receipts { get; set; } = new List<Receipt>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
