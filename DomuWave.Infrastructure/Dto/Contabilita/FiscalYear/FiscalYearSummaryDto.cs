namespace DomuWave.Services.Dto.Contabilita.FiscalYear
{
    /// <summary>
    /// Riepilogo finanziario aggregato dell'esercizio.
    /// Popolato nel dettaglio endpoint per fornire una vista consolidata
    /// dello stato economico del periodo.
    /// </summary>
    public class FiscalYearSummaryDto
    {
        /// <summary>Saldo iniziale dell'esercizio: somma dei saldi iniziali delle unità
        /// (propagati dal saldo di chiusura dell'esercizio precedente, se esistente).</summary>
        public decimal TotalOpeningBalance { get; set; }

        /// <summary>Totale spese registrate nell'esercizio (tutte, indipendentemente dallo stato).</summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>Totale spese effettivamente pagate.</summary>
        public decimal TotalExpensesPaid { get; set; }

        /// <summary>Totale importo rate emesse ai condomini.</summary>
        public decimal TotalInstallmentsBilled { get; set; }

        /// <summary>Totale incassi ricevuti dai condomini.</summary>
        public decimal TotalPaymentsReceived { get; set; }

        /// <summary>Saldo: incassi - spese pagate. Positivo = avanzo, negativo = disavanzo.</summary>
        public decimal Balance { get; set; }

        /// <summary>Numero di spese registrate nell'esercizio.</summary>
        public int ExpenseCount { get; set; }

        /// <summary>Numero di rate generate nell'esercizio.</summary>
        public int InstallmentCount { get; set; }

        /// <summary>Numero di bilanci (preventivi/consuntivi) associati all'esercizio.</summary>
        public int BudgetCount { get; set; }
    }
}
