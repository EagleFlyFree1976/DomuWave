namespace DomuWave.Services.Dto.Budget;

public class ApproveInstallmentOptionsDto
{
    public int      NumberOfInstallments { get; set; } = 4;
    public DateTime FirstDueDate         { get; set; } = DateTime.Today;
}
