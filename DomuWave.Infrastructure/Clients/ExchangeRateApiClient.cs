using System.Text.Json;
using DomuWave.Services.Settings;
using Microsoft.Extensions.Options;

namespace DomuWave.Services.Clients;

public class ExchangeRateApiClient : IExchangeRateClient
{
    private readonly DomuWaveSettings _settings;


    public ExchangeRateApiClient(IOptions<DomuWaveSettings> options)
    {
        _settings = options.Value;
    }
    
    public async Task<Dictionary<string, decimal>> GetExchangeRates(string baseCurrency, CancellationToken cancellationToken)
    {
        using HttpClient client = new HttpClient();
        string apiKey = _settings.ExchangeRateApyKey;
        string baseUrl = $"{_settings.ExchangeRateApiUrl}/{apiKey}/latest/";

        try
        {
            var response = await client.GetAsync(baseUrl + baseCurrency);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            if (doc.RootElement.GetProperty("result").GetString() == "success")
            {
                var conversionRates = doc.RootElement.GetProperty("conversion_rates");
                var rates = new Dictionary<string, decimal>();

                foreach (var property in conversionRates.EnumerateObject())
                {
                    rates[property.Name] = property.Value.GetDecimal();
                }

                return rates;
            }
            else
            {
                Console.WriteLine("Errore nella risposta dell'API");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore: {ex.Message}");
            return null;
        }
    }
    
    public async Task<Dictionary<string, decimal>> GetHistoricaltExchangeRates(string baseCurrency, DateTime dateTime,CancellationToken cancellationToken)
    {
        using HttpClient client = new HttpClient();
        string apiKey = _settings.ExchangeRateApyKey;
        string baseUrl = $"{_settings.ExchangeRateApiUrl}/{apiKey}/history/";

        try
        {
            string requestUri = $"{baseUrl}{baseCurrency}/{dateTime.Year}/{dateTime.Month}/{dateTime.Day}/1";
            var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            if (doc.RootElement.GetProperty("result").GetString() == "success")
            {
                var conversionRates = doc.RootElement.GetProperty("conversion_rates");
                var rates = new Dictionary<string, decimal>();

                foreach (var property in conversionRates.EnumerateObject())
                {
                    rates[property.Name] = property.Value.GetDecimal();
                }

                return rates;
            }
            else
            {
                Console.WriteLine("Errore nella risposta dell'API");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore: {ex.Message}");
            return null;
        }
    }
}