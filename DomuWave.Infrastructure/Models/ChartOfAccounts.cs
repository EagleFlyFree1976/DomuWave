using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Domain.Models
{
    public class ChartOfAccounts : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Code { get; set; }
        public virtual string Type { get; set; }
        public virtual string Category { get; set; }
        public virtual ChartOfAccounts ParentAccount { get; set; }
        public virtual int Level { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual IList<ChartOfAccounts> ChildAccounts { get; set; } = new List<ChartOfAccounts>();
        public virtual IList<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
        public virtual IList<Expense> Expenses { get; set; } = new List<Expense>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
