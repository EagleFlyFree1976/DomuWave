namespace DomuWave.Services.Clients;

public interface IExchangeRateClient
{
    Task<Dictionary<string, decimal>> GetExchangeRates(string basecurrencyCode, CancellationToken cancellationToken);

    Task<Dictionary<string, decimal>> GetHistoricaltExchangeRates(string baseCurrency, DateTime dateTime,
        CancellationToken cancellationToken);
}