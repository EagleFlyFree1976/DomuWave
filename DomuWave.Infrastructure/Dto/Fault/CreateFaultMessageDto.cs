namespace DomuWave.Services.Dto.Fault;

public class CreateFaultMessageDto
{
    public int    FaultId { get; set; }
    public string Body    { get; set; } = string.Empty;
}
