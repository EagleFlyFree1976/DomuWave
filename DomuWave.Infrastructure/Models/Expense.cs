using System;
namespace DomuWave.Services.Models
{
    public class Expense
    {
        public virtual long ExpenseId { get; set; }
        public virtual Guid TenantId { get; set; }
        public virtual int CondominiumId { get; set; }
        public virtual int AccountId { get; set; }
        public virtual int? SupplierId { get; set; }
        public virtual string DocumentNumber { get; set; }
        public virtual DateTime DocumentDate { get; set; }
        public virtual DateTime RegistrationDate { get; set; }
        public virtual string Description { get; set; }
        public virtual decimal GrossAmount { get; set; }
        public virtual decimal VatAmount { get; set; }
        public virtual decimal NetAmount { get; set; }
        public virtual string ExpenseType { get; set; }
        public virtual int MillesimalTableId { get; set; }
        public virtual string PaymentStatus { get; set; }
        public virtual DateTime? PaymentDate { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string DocumentPath { get; set; }
        public virtual string Notes { get; set; }
        
        
        
        
        
        
        
        public virtual Tenant Tenant { get; set; }
        public virtual Condominium Condominium { get; set; }
        public virtual ChartOfAccounts Account { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual MillesimalTable MillesimalTable { get; set; }
        public Expense()
        {
            RegistrationDate = DateTime.UtcNow;
            VatAmount = 0;
            PaymentStatus = "ToPay";
            IsDeleted = false;
            CreationDate = DateTime.UtcNow;
        }
    }
}
