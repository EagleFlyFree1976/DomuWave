using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.Currency;

public class ExchangeRateHistoryReadDto : TraceEntityDTO<long>
{
    public LookupEntityDto<int> FromCurrency { get; set; }
    public LookupEntityDto<int>  ToCurrency { get; set; }

    public virtual decimal Rate { get; set; }

    public virtual DateTime ValidFrom { get; set; }
    public virtual DateTime? ValidTo { get; set; }

}