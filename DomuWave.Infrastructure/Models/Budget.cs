using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Budget : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual FiscalYear FiscalYear { get; set; }
        public virtual string Type { get; set; }
        public virtual DateTime? ApprovalDate { get; set; }
        public virtual string Status { get; set; }
        public virtual decimal TotalIncome { get; set; }
        public virtual decimal TotalExpenses { get; set; }
        public virtual string Notes { get; set; }

        public virtual IList<BudgetItem> Items { get; set; } = new List<BudgetItem>();
        public virtual IList<CondominiumInstallment> Installments { get; set; } = new List<CondominiumInstallment>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
