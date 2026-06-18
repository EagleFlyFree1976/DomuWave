namespace DomuWave.Services.Dto.Expense
{
    /// <summary>
    /// Riepilogo aggregato delle spese di un condominio per un anno fiscale.
    /// Usato dal tool AI get_expense_summary.
    /// </summary>
    public class ExpenseSummaryDto
    {
        public int CondominiumId { get; set; }
        public int Year { get; set; }

        /// <summary>Numero di documenti di spesa nell'anno.</summary>
        public int DocumentCount { get; set; }

        /// <summary>Totale lordo di tutte le spese.</summary>
        public decimal TotalGrossAmount { get; set; }

        /// <summary>Totale lordo delle spese non ancora pagate.</summary>
        public decimal UnpaidGrossAmount { get; set; }

        /// <summary>Numero di spese non ancora pagate.</summary>
        public int UnpaidCount { get; set; }
    }
}
