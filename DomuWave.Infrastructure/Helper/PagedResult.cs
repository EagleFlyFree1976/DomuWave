using Microsoft.Extensions.Caching.Memory;

namespace DomuWave.Services.Helper;

public class PagedResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IList<T> Items { get; set; } = new List<T>();
}

 