namespace DomuWave.Application.Models;

public class CurrencyCreateUpdateDto
{
    public string Code { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public int DecimalDigits { get; set; }

    public bool IsActive { get; set; }
}

public class ExchangeRateHistoryCreateUpdateDto
{
    
    public int ToCurrencyId { get; set; }

    public decimal Rate { get; set; }

    public DateTime TargetDate { get; set; }
    
}