using CPQ.Core;

namespace DomuWave.Services.Models.Dto.Currency;

public class ExchangeRateHistoryCreateUpdateDto
{
    public int FromCurrencyId { get; set; }
    public int ToCurrencyId { get; set; }

    public decimal Rate { get; set; }

    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}