using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Expense : TenantEntity<long>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual ChartOfAccounts Account { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual string DocumentNumber { get; set; }
        public virtual DateTime DocumentDate { get; set; }
        public virtual DateTime RegistrationDate { get; set; }
        public virtual decimal  TaxableAmount        { get; set; }
        public virtual decimal  TaxableAmountVatExempt { get; set; }
        public virtual decimal  GrossAmount          { get; set; }
        public virtual decimal  VatAmount            { get; set; }
        public virtual decimal  NetAmount            { get; set; }
        public virtual decimal  PensionFund          { get; set; }
        public virtual decimal  WithholdingTax       { get; set; }
        public virtual decimal  StampDuty            { get; set; }
        public virtual ExpenseType ExpenseType { get; set; }

        public virtual FiscalYear FiscalYear { get; set; }
        public virtual MillesimalTable MillesimalTable { get; set; }
        public virtual ExpensePaymentStatus PaymentStatus { get; set; }
        public virtual DateTime? PaymentDate { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string DocumentPath { get; set; }
        public virtual string Notes { get; set; }
        public virtual ChargeabilityType ChargeabilityType { get; set; }

        public virtual IList<ExpenseAllocation> Allocations { get; set; } = new List<ExpenseAllocation>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
