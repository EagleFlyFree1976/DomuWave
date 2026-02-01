using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto;

public class BookEntityDto<T> : TraceEntityDTO<T>
{
    public string Name { get; set; }
    public string Description { get; set; }

    public long? BookId { get; set; }

    public int CurrencyId { get; set; }
}