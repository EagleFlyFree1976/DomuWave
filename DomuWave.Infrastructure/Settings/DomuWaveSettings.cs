namespace DomuWave.Services.Settings;

public  class DomuWaveSettings
{
    public string ExchangeRateApyKey { get; set; }

    public string ExchangeRateApiUrl { get; set; }

        
    public string DefaultCurrencyCode { get; set; }


        
    public CacheTimeouts CacheTimeouts { get; set; }

}