using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Receipt : TenantEntity<long>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual CondominiumFee Fee { get; set; }
        public virtual long UserId { get; set; }
        public virtual RealEstateUnit Unit { get; set; }
        public virtual DateTime ReceiptDate { get; set; }
        public virtual DateTime RegistrationDate { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string BankReference { get; set; }
        public virtual string ReconciliationStatus { get; set; }
        public virtual string Notes { get; set; }
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
