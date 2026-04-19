using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class MillesimalTable : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; } = null!;
        public virtual string Code { get; set; } = string.Empty;
        public virtual new string Name { get; set; } = string.Empty;
        public virtual new string? Description { get; set; }
        public virtual decimal TotalMillesimal { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsDraft { get; set; }
        public virtual new bool IsEnabled { get; set; }

        public virtual IList<UnitMillesimal> UnitMillesimals { get; set; } = new List<UnitMillesimal>();
        public virtual IList<Expense> Expenses { get; set; } = new List<Expense>(); 
        
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
