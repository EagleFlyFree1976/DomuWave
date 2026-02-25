using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class SupplierContract : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual Supplier Supplier { get; set; }
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

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
