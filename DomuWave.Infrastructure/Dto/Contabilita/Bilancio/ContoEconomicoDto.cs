namespace DomuWave.Services.Dto.Contabilita.Bilancio;

/// <summary>
/// Conto economico dell'esercizio (criterio di COMPETENZA).
/// Entrate = versamenti dei condòmini nel periodo; Uscite = spese contabilizzate
/// nell'esercizio (pagate o meno). Il saldo a pareggio è l'avanzo/disavanzo che
/// si trascina sull'esercizio successivo.
/// </summary>
public class ContoEconomicoDto
{
    public int      FiscalYearId    { get; set; }
    public string?  FiscalYearCode  { get; set; }
    public int      CondominiumId   { get; set; }
    public string?  CondominiumName { get; set; }
    public DateTime StartDate       { get; set; }
    public DateTime EndDate         { get; set; }

    // ── ENTRATE ──────────────────────────────────────────────────────────────
    /// <summary>Versamenti dei condòmini con data di pagamento nel periodo.</summary>
    public decimal  VersamentiCondomini { get; set; }
    /// <summary>Altre entrate contabilizzate (conti di tipo Entrata diversi dalle quote).</summary>
    public List<ContoEconomicoRowDto> EntrateRows { get; set; } = [];
    public decimal  TotaleEntrate   { get; set; }

    // ── USCITE ───────────────────────────────────────────────────────────────
    /// <summary>Spese di gestione per conto (competenza esercizio).</summary>
    public List<ContoEconomicoRowDto> UsciteRows { get; set; } = [];
    public decimal  TotaleUscite    { get; set; }

    // ── SALDO ────────────────────────────────────────────────────────────────
    /// <summary>Saldo dell'esercizio precedente (positivo = credito a favore, negativo = debito).</summary>
    public decimal  SaldoEsercizioPrecedente { get; set; }

    /// <summary>
    /// Saldo finale = TotaleEntrate - TotaleUscite + SaldoEsercizioPrecedente.
    /// &gt; 0 = avanzo (credito da restituire); &lt; 0 = disavanzo (debito da richiedere).
    /// </summary>
    public decimal  SaldoFinale     { get; set; }

    /// <summary>"Avanzo" | "Disavanzo" | "Pareggio"</summary>
    public string   SaldoTipo       { get; set; } = string.Empty;
}

public class ContoEconomicoRowDto
{
    public int     AccountId   { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal Amount      { get; set; }
}
