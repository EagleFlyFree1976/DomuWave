namespace DomuWave.Services.Dto.Condominium;

/// <summary>
/// DTO per la creazione di un nuovo Condominium.
/// </summary>
public class CreateCondominiumDto
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string TaxCode { get; set; }
    public string VatNumber { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Pec { get; set; }

    public int NumberOfUnits { get; set; }
    public int NumberOfStaircases { get; set; }
    public int? NumberOfFloors { get; set; }
    public int? YearOfConstruction { get; set; }
    public decimal TotalMillesimal { get; set; }
    public bool HasElevator { get; set; }
    public int? NumberOfElevators { get; set; }
    public bool HasCentralHeating { get; set; }
    public bool HasConcierge { get; set; }
    public decimal? CommonAreasSqm { get; set; }

    public DateTime? MandateStartDate { get; set; }
    public DateTime? MandateEndDate { get; set; }
    public DateTime? LastAssemblyDate { get; set; }

    public string InstallmentFrequency { get; set; }
    public int InstallmentDueDay { get; set; }

    public string Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public CondominiumAddressDto Address { get; set; }

    // ── Dati bancari ──────────────────────────────────────────────────────────
    public string? Iban               { get; set; }
    public string? BankAccountHolder  { get; set; }
    public string? BankName           { get; set; }

    // Saldo iniziale di cassa del condominio (disponibilità liquide di partenza).
    public decimal? InitialBalance    { get; set; }

    // ── Amministratore ────────────────────────────────────────────────────────
    public string? AdministratorName  { get; set; }
    public string? AdministratorPhone { get; set; }
    public string? AdministratorEmail { get; set; }
}
