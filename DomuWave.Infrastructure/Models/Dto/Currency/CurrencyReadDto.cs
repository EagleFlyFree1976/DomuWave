using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.Currency;

public class CurrencyReadDto : TraceEntityDTO<int>
{
    public   string Code { get; set; }
    public   string Symbol { get; set; }
    public   string Name { get; set; }

    public string Description
    {
        get { return Name; }
    }
    public   int DecimalDigits { get; set; }

    

    

}