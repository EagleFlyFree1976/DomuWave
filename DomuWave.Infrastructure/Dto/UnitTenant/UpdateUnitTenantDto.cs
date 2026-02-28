namespace DomuWave.Services.Dto.UnitTenant;

public class UpdateUnitTenantDto
{
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public string? TaxCode { get; set; }
    public string? ExpensePayer { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
