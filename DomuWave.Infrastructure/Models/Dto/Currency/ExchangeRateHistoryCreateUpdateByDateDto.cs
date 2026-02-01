namespace DomuWave.Services.Models.Dto.Currency;

public class ExchangeRateHistoryCreateUpdateByDateDto
{
    public int FromCurrencyId { get; set; }
    public int ToCurrencyId { get; set; }

    public decimal Rate { get; set; }

    public DateTime TargetDate { get; set; }
}