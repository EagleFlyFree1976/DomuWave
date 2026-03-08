namespace DomuWave.Services.Dto.CondominiumInstallment;

public class UpdateCondominiumInstallmentDto
{
    public int      InstallmentNumber { get; set; }
    public DateTime DueDate           { get; set; }
    public decimal  TotalAmount       { get; set; }
    public int      StatusId          { get; set; }
    public string?  Notes             { get; set; }
}
