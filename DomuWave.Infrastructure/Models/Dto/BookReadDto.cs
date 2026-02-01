using CPQ.Core;
using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto;

public class BookReadDto : TraceEntityDTO<long>
{
 
    public string Name { get; set; }
    public string Description { get; set; }

    public bool IsPrimary { get; set; }
    public long OwnerId { get; set; }

    public int CurrencyId { get; set; }
}