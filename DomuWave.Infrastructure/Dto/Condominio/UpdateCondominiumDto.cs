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
    public string InstallmentFrequency { get; set; }
    public int InstallmentDueDay { get; set; }
    public string Notes { get; set; }
    public bool IsActive { get; set; }
    public CondominiumAddressDto Address { get; set; }
}
