using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class RealEstateUnit : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual Building? Building { get; set; }
        public virtual Staircase? Staircase { get; set; }
        public virtual int Floor { get; set; }
        public virtual string InternalNumber { get; set; }
        public virtual string Sheet { get; set; }
        public virtual string Parcel { get; set; }
        public virtual string Subordinate { get; set; }
        public virtual string Category { get; set; }
        public virtual decimal? CadastralIncome { get; set; }
        public virtual decimal? AreaSqm { get; set; }
        public virtual decimal? Rooms { get; set; }
        public virtual string UnitType { get; set; }
        public virtual string OccupancyStatus { get; set; }
        public virtual string Notes { get; set; }
        /// <summary>
        /// Etichetta visualizzata (es. cognomi proprietari). Ricalcolata automaticamente
        /// quando si aggiungono/modificano proprietari, a meno che <see cref="IsDisplayNameOverridden"/>
        /// sia true, nel qual caso il valore impostato manualmente dall'utente ha priorità.
        /// </summary>
        public virtual string DisplayName { get; set; }
        /// <summary>
        /// Se true, <see cref="DisplayName"/> è stato impostato manualmente dall'utente
        /// e non deve essere sovrascritto dal ricalcolo automatico sui proprietari.
        /// </summary>
        public virtual bool IsDisplayNameOverridden { get; set; }
        public virtual int    NumeroAbitanti { get; set; } = 1;
        public virtual bool IsActive { get; set; }
        public virtual BillingGroup? BillingGroup { get; set; }

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
