using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Fault;

public class FaultMessageReadDto : TraceEntityDTO<int>
{
    public int    FaultId        { get; set; }
    public long   AuthorUserId  { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string Body           { get; set; } = string.Empty;
}
