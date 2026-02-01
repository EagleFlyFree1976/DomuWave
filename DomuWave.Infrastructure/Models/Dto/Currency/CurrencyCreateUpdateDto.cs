namespace DomuWave.Services.Models.Dto.Currency;

public class CurrencyCreateUpdateDto
{
    public string Code { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public int DecimalDigits { get; set; }

    public bool IsEnabled { get; set; }
}