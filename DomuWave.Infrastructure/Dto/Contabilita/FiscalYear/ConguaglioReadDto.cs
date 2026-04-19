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
/// Riga di conguaglio: può rappresentare una singola unità o un gruppo di fatturazione.
/// Se IsGroup == true, DisplayName è il nome del gruppo e Units contiene le righe delle singole unità.
/// </summary>
public class ConguaglioUnitItemDto
{
    /// <summary>Id dell'unità (0 se IsGroup == true).</summary>
    public int     UnitId             { get; set; }
    public string  UnitInternalNumber { get; set; } = string.Empty;
    public string  UnitDescription    { get; set; } = string.Empty;

    /// <summary>True se questa riga aggrega più unità tramite un BillingGroup.</summary>
    public bool    IsGroup            { get; set; }
    public int?    BillingGroupId     { get; set; }
    public string? BillingGroupName   { get; set; }

    /// <summary>Millesimi dell'unità (o somma del gruppo).</summary>
    public decimal Millesimal         { get; set; }

    /// <summary>Quota consuntiva: TotalExpenses * Millesimal / TotalMillesimal.</summary>
    public decimal QuotaConsuntiva    { get; set; }

    /// <summary>Totale già pagato sulle rate del preventivo.</summary>
    public decimal AlreadyPaid        { get; set; }

    /// <summary>Saldo = QuotaConsuntiva - AlreadyPaid. Positivo = debito; negativo = credito.</summary>
    public decimal Saldo              { get; set; }

    /// <summary>"Debit" | "Credit" | "Even"</summary>
    public string  SaldoType          { get; set; } = string.Empty;

    /// <summary>Sotto-righe delle singole unità quando IsGroup == true.</summary>
    public IList<ConguaglioUnitItemDto> Units { get; set; } = new List<ConguaglioUnitItemDto>();
}
