namespace DomuWave.Services.Dto.CondominiumInstallment;

public class UpdateCondominiumInstallmentDto
{
    public int      InstallmentNumber { get; set; }
    public DateTime DueDate           { get; set; }
    public decimal  TotalAmount       { get; set; }
    public string   Status            { get; set; } = string.Empty;
    public string?  Notes             { get; set; }
}
