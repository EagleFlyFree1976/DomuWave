namespace DomuWave.Services.Settings;

public  class DomuWaveSettings
{
    public string ExchangeRateApyKey { get; set; }

    public string ExchangeRateApiUrl { get; set; }


    public string DefaultCurrencyCode { get; set; }



    public CacheTimeouts CacheTimeouts { get; set; }

    /// <summary>SMTP di piattaforma (mail di sistema: verifica registrazione, ecc.)</summary>
    public PlatformSmtpSettings SmtpSettings { get; set; }

    /// <summary>URL base della pagina di verifica registrazione (il token viene aggiunto come query).</summary>
    public string RegistrationVerifyUrl { get; set; }
}

public class PlatformSmtpSettings
{
    public string Host      { get; set; } = string.Empty;
    public int    Port      { get; set; } = 587;
    public bool   UseSsl    { get; set; } = true;
    public string Username  { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName  { get; set; } = "DomuWave";
}