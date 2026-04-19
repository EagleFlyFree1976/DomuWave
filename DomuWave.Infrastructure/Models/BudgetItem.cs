using System;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class BudgetItem : TenantEntity<int>
    {
        public virtual Budget Budget { get; set; } = null!;
        public virtual ChartOfAccounts Account { get; set; } = null!;
        public virtual decimal Amount     { get; set; }
        public virtual decimal AmountPaid { get; set; }
        public virtual string? Notes { get; set; }

        /// <summary>
        /// Snapshot del codice conto al momento della creazione/ricalcolo.
        /// Non viene aggiornato quando il piano dei conti cambia, in modo che
        /// i budget approvati o chiusi mantengano la composizione originale.
        /// </summary>
        public virtual string AccountCode { get; set; } = string.Empty;

        /// <summary>
        /// Snapshot del nome conto al momento della creazione/ricalcolo.
        /// Vedi <see cref="AccountCode"/>.
        /// </summary>
        public virtual string AccountName { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
