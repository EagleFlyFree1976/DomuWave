namespace DomuWave.Application.Models;

public class AccountUpdateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    
    public int CurrencyId { get; set; }
    public  decimal? InitialBalance { get; set; }

    public DateTime OpenDate { get; set; }
    public DateTime? ClosedDate { get; set; }



}