namespace DomuWave.Services.Dto.UnitTenant;

public class UpdateUnitTenantDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
