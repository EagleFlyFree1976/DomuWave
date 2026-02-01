namespace DomuWave.Services.Models.Dto.Import;

public class ImportRowDto
{
    public  DateTime TransactionDate { get; set; }
    public  decimal? DepositAmount { get; set; }
    public decimal? WithdrawalAmount { get; set; }
    public decimal? Amount { get; set; }
            
    public  string? CategoryName { get; set; }
    public  string? SubCategoryName { get; set; }
    public  string? Description { get; set; }
    public  string? Currency { get; set; }
             
    public  string? Beneficiary { get; set; }
    public  string? Type { get; set; }
    public string? Status { get; set; }
}