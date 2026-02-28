namespace DomuWave.Services.Dto.Condominium;

/// <summary>
/// DTO per l'aggiornamento di un Condominium esistente.
/// </summary>
public class UpdateCondominiumDto
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
    public bool IsActive { get; set; }
    public CondominiumAddressDto Address { get; set; }
}
