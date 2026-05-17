using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.PrivateThread;

public class PrivateThreadReadDto : TraceEntityDTO<int>
{
    public int    CondominiumId          { get; set; }
    public string CondominiumName        { get; set; } = string.Empty;
    public long   CondominioUserId       { get; set; }
    public string CondominioUserFullName { get; set; } = string.Empty;
    public int    MessageCount           { get; set; }
    public int    UnreadCount            { get; set; }
    public string? LastMessageBody       { get; set; }
    public DateTime? LastMessageDate     { get; set; }
}
