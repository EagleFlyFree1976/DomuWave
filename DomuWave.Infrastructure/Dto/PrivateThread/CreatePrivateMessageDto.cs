namespace DomuWave.Services.Dto.PrivateThread;

public class CreatePrivateMessageDto
{
    public int    ThreadId { get; set; }
    public string Body     { get; set; } = string.Empty;
}
