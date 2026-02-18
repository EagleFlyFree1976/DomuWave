using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Domain.Models
{
    public class CondominiumInstallment : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual Budget Budget { get; set; }
        public virtual int Year { get; set; }
        public virtual int InstallmentNumber { get; set; }
        public virtual DateTime DueDate { get; set; }
        public virtual decimal TotalAmount { get; set; }
        public virtual string Status { get; set; }
        public virtual string Notes { get; set; }

        public virtual IList<CondominiumFee> Fees { get; set; } = new List<CondominiumFee>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
