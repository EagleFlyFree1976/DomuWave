namespace DomuWave.Services.Models.Dto;

public class AccountCreateDto
{
    public long BookId { get; set; }
    public int CurrencyId { get; set; }
    public int AccountTypeId { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public int OwnerId { get; set; }
        
    public decimal? InitialBalance { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}