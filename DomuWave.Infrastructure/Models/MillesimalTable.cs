using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class MillesimalTable : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual decimal TotalMillesimal { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsDraft { get; set; }
        public virtual bool IsEnabled { get; set; }

        public virtual IList<UnitMillesimal> UnitMillesimals { get; set; } = new List<UnitMillesimal>();
        public virtual IList<Expense> Expenses { get; set; } = new List<Expense>(); 
        
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
