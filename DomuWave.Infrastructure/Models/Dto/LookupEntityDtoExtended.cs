using CPQ.Core.DTO;

namespace DomuWave.Services.Models;

public class LookupEntityDtoExtended<T> : LookupEntityDto<T>
{
    public string CssClass { get; set; }
}