namespace DomuWave.Services.Dto.TenantSmtp;

public class UpsertTenantSmtpSettingsDto
{
    public string  Host      { get; set; } = string.Empty;
    public int     Port      { get; set; } = 587;
    public bool    UseSsl    { get; set; } = true;
    public string  Username  { get; set; } = string.Empty;
    public string? Password  { get; set; }   // null = non modificare
    public string  FromEmail { get; set; } = string.Empty;
    public string  FromName  { get; set; } = string.Empty;
    public bool    IsEnabled { get; set; } = true;
}
