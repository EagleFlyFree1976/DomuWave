using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.ChartOfAccounts;

public class CreateChartOfAccountsDto
{
    public int                 CondominiumId   { get; set; }
    public int?                ParentAccountId { get; set; }
    public string              Code            { get; set; } = string.Empty;
    public string              Name            { get; set; } = string.Empty;
    public ChartOfAccountsType Type            { get; set; } = ChartOfAccountsType.Uscita;
    public int?                CategoryId      { get; set; }
    public string?             Description     { get; set; }
    public bool                IsActive                  { get; set; } = true;
    public int?                DefaultMillesimalTableId  { get; set; }
    public AllocationMethod    AllocationMethod          { get; set; } = AllocationMethod.Standard;
    public decimal?            MillesimalPercentage      { get; set; }
    public decimal?            FloorWeight               { get; set; }
    public decimal?            InhabitantsWeight         { get; set; }
    public int                 ChargeabilityTypeId       { get; set; } = ChargeabilityType.Owner;
}
