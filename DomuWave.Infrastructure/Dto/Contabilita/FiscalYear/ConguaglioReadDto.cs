namespace DomuWave.Services.Dto.Contabilita.FiscalYear;

/// <summary>
/// Documento di conguaglio per un esercizio fiscale.
/// Confronta le quote consuntive reali con gli importi già pagati
/// tramite le rate del preventivo, producendo un saldo per unità.
/// </summary>
public class ConguaglioReadDto
{
    public int      FiscalYearId   { get; set; }
    public string   FiscalYearCode { get; set; }
    public int      CondominiumId  { get; set; }
    public string   CondominiumName { get; set; }

    /// <summary>Totale spese reali dell'esercizio (somma voci consuntivo approvato).</summary>
    public decimal TotalExpenses   { get; set; }

    /// <summary>Totale rate già incassate (AmountPaid su tutte le CondominiumFee del preventivo).</summary>
    public decimal TotalPaid       { get; set; }

    /// <summary>Saldo globale = TotalExpenses - TotalPaid. Positivo = debito residuo; negativo = credito.</summary>
    public decimal GlobalBalance   { get; set; }

    /// <summary>Data di approvazione del budget consuntivo.</summary>
    public DateTime? ApprovalDate  { get; set; }

    public IList<ConguaglioUnitItemDto> Units { get; set; } = new List<ConguaglioUnitItemDto>();
}

/// <summary>
/// Riga di conguaglio per una singola unità immobiliare.
/// </summary>
public class ConguaglioUnitItemDto
{
    public int     UnitId            { get; set; }
    public string  UnitInternalNumber { get; set; }
    public string  UnitDescription   { get; set; }

    /// <summary>Millesimi dell'unità nella tabella millesimale usata.</summary>
    public decimal Millesimal        { get; set; }

    /// <summary>Quota consuntiva: TotalExpenses * Millesimal / TotalMillesimal.</summary>
    public decimal QuotaConsuntiva   { get; set; }

    /// <summary>Totale già pagato dall'unità sulle rate del preventivo.</summary>
    public decimal AlreadyPaid       { get; set; }

    /// <summary>
    /// Saldo = QuotaConsuntiva - AlreadyPaid.
    /// Positivo = deve ancora pagare (conguaglio a debito).
    /// Negativo = ha pagato troppo (conguaglio a credito).
    /// </summary>
    public decimal Saldo             { get; set; }

    /// <summary>"Debit" | "Credit" | "Even"</summary>
    public string  SaldoType         { get; set; }
}
