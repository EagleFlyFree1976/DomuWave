using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.PrivateThread;

public class PrivateMessageReadDto : TraceEntityDTO<int>
{
    public int    ThreadId          { get; set; }
    public long   SenderUserId      { get; set; }
    public string SenderFullName    { get; set; } = string.Empty;
    public string Body              { get; set; } = string.Empty;
    public bool   IsReadByRecipient { get; set; }
}
