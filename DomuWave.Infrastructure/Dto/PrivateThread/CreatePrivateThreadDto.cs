namespace DomuWave.Services.Dto.PrivateThread;

public class CreatePrivateThreadDto
{
    public int    CondominiumId    { get; set; }
    public long   CondominioUserId { get; set; }
    public string FirstMessage     { get; set; } = string.Empty;
}
