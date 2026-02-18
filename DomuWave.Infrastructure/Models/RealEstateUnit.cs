using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Domain.Models
{
    public class RealEstateUnit : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Staircase { get; set; }
        public virtual int Floor { get; set; }
        public virtual string InternalNumber { get; set; }
        public virtual string Subordinate { get; set; }
        public virtual string Category { get; set; }
        public virtual decimal? CadastralIncome { get; set; }
        public virtual decimal? AreaSqm { get; set; }
        public virtual decimal? Rooms { get; set; }
        public virtual string UnitType { get; set; }
        public virtual string OccupancyStatus { get; set; }
        public virtual string Notes { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual IList<UnitOwner> Owners { get; set; } = new List<UnitOwner>();
        public virtual IList<UnitTenant> Tenants { get; set; } = new List<UnitTenant>();
        public virtual IList<UnitMillesimal> Millesimals { get; set; } = new List<UnitMillesimal>();
        public virtual IList<ExpenseAllocation> ExpenseAllocations { get; set; } = new List<ExpenseAllocation>();
        public virtual IList<CondominiumFee> Fees { get; set; } = new List<CondominiumFee>();
        public virtual IList<Receipt> Receipts { get; set; } = new List<Receipt>();
        
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
