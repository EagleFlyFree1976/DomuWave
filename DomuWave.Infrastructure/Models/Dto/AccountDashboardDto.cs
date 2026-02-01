using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace DomuWave.Services.Models.Dto
{
    public class AccountDashboardDto
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }

        public DateTime OpenDate { get; set; }
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// Saldo account
        /// </summary>
        public decimal? AccountBalance { get; set; }


        /// <summary>
        /// Saldo disponibile comprensivo di movimenti non ancora contabilizzati
        /// </summary>
        public decimal? AvailableBalance { get; set; }


        public List<MovementTotal> Totals { get; set; } = new List<MovementTotal>();


    }

    public enum MovementType : int
    {
        Deposit = 0,
        Expense = 1,
        TrasferIn = 2,
        TrasferOut = 3
    }

    public enum Period : int
    {
        CurrentDay,
        CurrentWeek,
        CurrentMonth,
        CurrentQuarter,
        PreviousDay,
        PreviousWeek,
        PreviousMonth,
        PreviousQuarter

    }

    public class PeriodDefinition
    {
        public Period Period { get; set; }

        public DateTime LastDay { get; set; }
        public DateTime FirstDay { get; set; }
    }
    public class MovementTotal
    {
        public MovementType MovementType  { get; set; }
        public PeriodDefinition Period { get; set; }

        /// <summary>
        /// Ammontare dei movimenti
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Numero di movimenti
        /// </summary>
        public int Total { get; set; }
    }
}
