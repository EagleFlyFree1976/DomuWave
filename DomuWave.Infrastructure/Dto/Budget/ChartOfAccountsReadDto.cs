using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.Budget;

public class ChartOfAccountsReadDto
{
    public int                  Id              { get; set; }
    public Guid                 TenantId        { get; set; }
    public int                  CondominiumId   { get; set; }
    public string               Code            { get; set; } = string.Empty;
    public string               Name            { get; set; } = string.Empty;
    public ChartOfAccountsType  Type            { get; set; }
    public int?                 CategoryId      { get; set; }
    public string?              CategoryName    { get; set; }
    public int                  Level           { get; set; }
    public bool                 IsActive        { get; set; }
    public bool                 IsLiquidity     { get; set; }
    public int?                 ParentAccountId             { get; set; }
    public int?                 DefaultMillesimalTableId    { get; set; }
    public string?              DefaultMillesimalTableName  { get; set; }
    public AllocationMethod     AllocationMethod            { get; set; }
    public decimal?             MillesimalPercentage        { get; set; }
    public decimal?             FloorWeight                 { get; set; }
    public decimal?             InhabitantsWeight           { get; set; }
    public int                  ChargeabilityTypeId         { get; set; }
    public string               ChargeabilityTypeName       { get; set; } = string.Empty;
}
