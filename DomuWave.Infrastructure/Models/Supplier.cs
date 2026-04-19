using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Supplier : TenantEntity<int>
    {
        public virtual string CompanyName { get; set; } = string.Empty;
        public virtual string? VatNumber { get; set; }
        public virtual string? TaxCode { get; set; }
        public virtual string? Address { get; set; }
        public virtual string? City { get; set; }
        public virtual string? Province { get; set; }
        public virtual string? PostalCode { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? Phone { get; set; }
        public virtual string? Pec { get; set; }
        public virtual string? ContactPerson { get; set; }
        public virtual string? SupplierType { get; set; }
        public virtual string? PaymentTerms { get; set; }
        public virtual string? IbanAccount { get; set; }
        public virtual string? Notes { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual IList<SupplierContract> Contracts { get; set; } = new List<SupplierContract>();
        public virtual IList<Expense> Expenses { get; set; } = new List<Expense>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
