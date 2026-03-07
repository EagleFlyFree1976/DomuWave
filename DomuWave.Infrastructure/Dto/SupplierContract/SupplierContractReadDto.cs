using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.SupplierContract;

public class SupplierContractReadDto : TraceEntityDTO<int>
{
    public int     CondominiumId   { get; set; }
    public string? CondominiumName { get; set; }
    public int     SupplierId      { get; set; }
    public string? SupplierName    { get; set; }
    public string? ContractNumber  { get; set; }
    public string  Subject         { get; set; } = string.Empty;
    public DateTime  StartDate     { get; set; }
    public DateTime? EndDate       { get; set; }
    public decimal?  AnnualAmount  { get; set; }
    public string?   Frequency     { get; set; }
    public bool      AutoRenewal   { get; set; }
    public string?   Status        { get; set; }
    public string?   DocumentPath  { get; set; }
    public string?   Notes         { get; set; }
}
