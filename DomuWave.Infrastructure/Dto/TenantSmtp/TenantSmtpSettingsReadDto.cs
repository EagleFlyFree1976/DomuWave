using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.TenantSmtp;

public class TenantSmtpSettingsReadDto : TraceEntityDTO<int>
{
    public string Host      { get; set; } = string.Empty;
    public int    Port      { get; set; }
    public bool   UseSsl    { get; set; }
    public string Username  { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName  { get; set; } = string.Empty;
    public bool   IsEnabled { get; set; }
}
