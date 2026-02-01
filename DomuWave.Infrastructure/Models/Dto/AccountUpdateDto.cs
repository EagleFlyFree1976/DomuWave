namespace DomuWave.Services.Models.Dto;

public class AccountUpdateDto
{
    public long AccountId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal? InitialBalance { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    public int CurrencyId { get; set; }
    //public DateTime? ClosedDate { get; set; }
}