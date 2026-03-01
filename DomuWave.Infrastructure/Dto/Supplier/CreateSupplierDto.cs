namespace DomuWave.Services.Dto.Supplier;

public class CreateSupplierDto
{
    public string  CompanyName   { get; set; }
    public string? VatNumber     { get; set; }
    public string? TaxCode       { get; set; }
    public string? Address       { get; set; }
    public string? City          { get; set; }
    public string? Province      { get; set; }
    public string? PostalCode    { get; set; }
    public string? Email         { get; set; }
    public string? Phone         { get; set; }
    public string? Pec           { get; set; }
    public string? ContactPerson { get; set; }
    public string? SupplierType  { get; set; }
    public string? PaymentTerms  { get; set; }
    public string? IbanAccount   { get; set; }
    public string? Notes         { get; set; }
    public bool    IsActive      { get; set; } = true;
}
