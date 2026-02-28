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
    public string InstallmentFrequency { get; set; }
    public int InstallmentDueDay { get; set; }
    public string Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public CondominiumAddressDto Address { get; set; }
}
