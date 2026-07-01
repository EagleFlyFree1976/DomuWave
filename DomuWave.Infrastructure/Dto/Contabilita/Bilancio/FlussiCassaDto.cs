namespace DomuWave.Services.Dto.Contabilita.Bilancio;

/// <summary>
/// Flussi di cassa dell'esercizio (criterio di CASSA).
/// Sintetizza incassi e pagamenti effettivamente avvenuti nel periodo, partendo
/// dall'avanzo di cassa iniziale fino all'avanzo finale (disponibilità reale).
/// I pagamenti sono distinti per esercizio di competenza (ordinario / precedenti).
/// </summary>
public class FlussiCassaDto
{
    public int      FiscalYearId    { get; set; }
    public string?  FiscalYearCode  { get; set; }
    public int      CondominiumId   { get; set; }
    public string?  CondominiumName { get; set; }
    public DateTime StartDate       { get; set; }
    public DateTime EndDate         { get; set; }

    // ── INCASSI ──────────────────────────────────────────────────────────────
    /// <summary>Avanzo di cassa all'inizio del periodo (somma OpeningBalance conti liquidi).</summary>
    public decimal  AvanzoInizialeCassa { get; set; }
    /// <summary>Versamenti dei condòmini con data di pagamento nel periodo.</summary>
    public decimal  VersamentiCondomini { get; set; }
    /// <summary>Altri incassi del periodo: movimenti su conti di tipo Entrata (rimborsi, interessi, ecc.).</summary>
    public decimal  AltriIncassi    { get; set; }
    public decimal  TotaleIncassi   { get; set; }

    // ── PAGAMENTI ────────────────────────────────────────────────────────────
    /// <summary>Spese pagate nel periodo di competenza dell'esercizio corrente.</summary>
    public decimal  PagamentiEsercizioCorrente   { get; set; }
    /// <summary>Spese pagate nel periodo ma di competenza di esercizi precedenti.</summary>
    public decimal  PagamentiEserciziPrecedenti  { get; set; }
    /// <summary>Uscite individuali (spese imputate a singola unità) pagate nel periodo.</summary>
    public decimal  UsciteIndividuali { get; set; }
    public decimal  TotalePagamenti { get; set; }

    // ── SALDO ────────────────────────────────────────────────────────────────
    /// <summary>Avanzo di cassa al termine del periodo = TotaleIncassi - TotalePagamenti.</summary>
    public decimal  AvanzoFinaleCassa { get; set; }
}
