using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class ChartOfAccounts : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Code { get; set; }
        public virtual ChartOfAccountsType Type { get; set; }
        public virtual ChartOfAccountsCategory? Category { get; set; }
        public virtual ChartOfAccounts ParentAccount { get; set; }
        public virtual int Level { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual MillesimalTable? DefaultMillesimalTable { get; set; }

        public virtual AllocationMethod   AllocationMethod    { get; set; } = AllocationMethod.Standard;
        public virtual decimal?           MillesimalPercentage { get; set; }
        public virtual decimal?           FloorWeight          { get; set; }
        public virtual decimal?           InhabitantsWeight    { get; set; }

        public virtual ChargeabilityType  ChargeabilityType   { get; set; }

        public virtual IList<ChartOfAccounts> ChildAccounts { get; set; } = new List<ChartOfAccounts>();
        public virtual IList<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
        public virtual IList<Expense> Expenses { get; set; } = new List<Expense>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
            
        }
    }
}
