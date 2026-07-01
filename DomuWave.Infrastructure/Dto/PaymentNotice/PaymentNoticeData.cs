namespace DomuWave.Services.Dto.PaymentNotice;

/// <summary>
/// Dati aggregati necessari per generare un avviso di pagamento PDF.
/// Usato sia per l'avviso per singola unità (tutte le rate dell'esercizio)
/// sia per l'avviso batch per tutte le unità di una singola rata.
/// </summary>
public class PaymentNoticeData
{
    // ── Condominio ────────────────────────────────────────────────────────────
    public string CondominiumName    { get; set; } = string.Empty;
    public string CondominiumCode    { get; set; } = string.Empty;
    public string CondominiumTaxCode { get; set; } = string.Empty;
    public string CondominiumAddress { get; set; } = string.Empty;
    public string CondominiumEmail   { get; set; } = string.Empty;
    public string CondominiumPhone   { get; set; } = string.Empty;

    // ── Dati bancari ──────────────────────────────────────────────────────────
    public string Iban               { get; set; } = string.Empty;
    public string BankAccountHolder  { get; set; } = string.Empty;
    public string BankName           { get; set; } = string.Empty;

    // ── Amministratore ────────────────────────────────────────────────────────
    public string AdministratorName  { get; set; } = string.Empty;
    public string AdministratorPhone { get; set; } = string.Empty;
    public string AdministratorEmail { get; set; } = string.Empty;

    // ── Esercizio fiscale ─────────────────────────────────────────────────────
    public string FiscalYearCode { get; set; } = string.Empty;

    // ── Unità immobiliare ─────────────────────────────────────────────────────
    public string UnitInternalNumber { get; set; } = string.Empty;
    public string UnitDisplayName    { get; set; } = string.Empty;
    public string UnitStaircase      { get; set; } = string.Empty;
    public int    UnitFloor          { get; set; }

    // ── Proprietario ──────────────────────────────────────────────────────────
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerEmail    { get; set; } = string.Empty;

    // ── Righe rate ────────────────────────────────────────────────────────────
    public List<PaymentNoticeRow> Rows { get; set; } = [];

    // ── Totali ────────────────────────────────────────────────────────────────
    public decimal TotalDue     => Rows.Sum(r => r.AmountDue);
    public decimal TotalPaid    => Rows.Sum(r => r.AmountPaid);
    public decimal TotalBalance => Rows.Sum(r => r.Balance);

    // ── Contenuto libero (per lettere/comunicazioni) ──────────────────────────
    public string? CustomSubject { get; set; }
    public string? CustomBody    { get; set; }

    // ── Logo del tenant (opzionale, mostrato in intestazione) ─────────────────
    public byte[]? LogoContent { get; set; }

    // ── Metadati ──────────────────────────────────────────────────────────────
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

public class PaymentNoticeRow
{
    public int      InstallmentNumber { get; set; }
    public DateTime DueDate           { get; set; }
    public decimal  AmountDue         { get; set; }
    public decimal  AmountPaid        { get; set; }
    public decimal  Balance           { get; set; }
    public string   PaymentStatus     { get; set; } = string.Empty;
    public DateTime? PaymentDate      { get; set; }
    public string   PaymentCode       { get; set; } = string.Empty;
}
