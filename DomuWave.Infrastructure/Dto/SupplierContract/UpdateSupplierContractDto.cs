using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.SupplierContract;

public class UpdateSupplierContractDto
{
    [Required]
    public string  Subject        { get; set; } = string.Empty;
    public string? ContractNumber { get; set; }
    [Required]
    public DateTime  StartDate    { get; set; }
    public DateTime? EndDate      { get; set; }
    public decimal?  AnnualAmount { get; set; }
    public string?   Frequency    { get; set; }
    public bool      AutoRenewal  { get; set; }
    public string?   Status       { get; set; }
    public string?   DocumentPath { get; set; }
    public string?   Notes        { get; set; }
}
