namespace DomuWave.Services.Dto.Contabilita.Bilancio;

/// <summary>
/// Situazione patrimoniale (criterio di CASSA): fotografia di attività e passività
/// alla data di fine esercizio. Deve chiudere in pareggio (Totale Attività = Totale Passività).
/// </summary>
public class SituazionePatrimonialeDto
{
    public int      FiscalYearId    { get; set; }
    public string?  FiscalYearCode  { get; set; }
    public int      CondominiumId   { get; set; }
    public string?  CondominiumName { get; set; }
    public DateTime ReferenceDate   { get; set; }

    // ── ATTIVITÀ ─────────────────────────────────────────────────────────────
    /// <summary>Crediti verso condòmini (rate dovute e non ancora versate).</summary>
    public decimal  CreditiVersoCondomini { get; set; }
    /// <summary>Disponibilità liquide (conti Patrimoniali con IsLiquidity = true).</summary>
    public List<SituazionePatrimonialeRowDto> Disponibilita { get; set; } = [];
    public decimal  TotaleAttivita  { get; set; }

    // ── PASSIVITÀ ────────────────────────────────────────────────────────────
    /// <summary>Debiti verso condòmini (saldi a credito dei condòmini).</summary>
    public decimal  DebitiVersoCondomini { get; set; }
    /// <summary>Debiti verso terzi (spese contabilizzate e non ancora pagate).</summary>
    public decimal  DebitiVersoTerzi { get; set; }
    /// <summary>Fondi accantonati (conti Patrimoniali con IsLiquidity = false).</summary>
    public List<SituazionePatrimonialeRowDto> Fondi { get; set; } = [];
    public decimal  TotalePassivita { get; set; }

    /// <summary>
    /// Differenza di pareggio = TotaleAttivita - TotalePassivita. In un bilancio corretto è 0;
    /// uno scostamento segnala dati incompleti (es. fondi/saldi iniziali non valorizzati).
    /// </summary>
    public decimal  Sbilancio       { get; set; }
}

public class SituazionePatrimonialeRowDto
{
    public int     AccountId   { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal Amount      { get; set; }
}
