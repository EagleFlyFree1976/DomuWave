using DomuWave.Services.Clients;
using DomuWave.Services.Command.Currency.ExchangeRate;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Currency;
using DomuWave.Services.Settings;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency.Exchange;

public class FillExchangeRateCommandConsumer : InMemoryConsumerBase<FillExchangeRateCommand, bool>
{
    private readonly ILogger<FillExchangeRateCommandConsumer> _logger;
    private readonly ICurrencyService _currencyService;
    private readonly IUserService _userService;
    private readonly IExchangeRateClient _exchangeRateClient;
    private readonly IExchangeRateHistoryService _exchangeRateHistoryService;
    private readonly DomuWaveSettings _settings;
    public FillExchangeRateCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IExchangeRateClient exchangeRateClient, IUserService userService, ICurrencyService currencyService, 
        IExchangeRateHistoryService exchangeRateHistoryService, ILogger<FillExchangeRateCommandConsumer> logger, IOptions<DomuWaveSettings> options) : base(sessionFactoryProvider)
    {
        _exchangeRateClient = exchangeRateClient;
        _userService = userService;
        _currencyService = currencyService;
        _exchangeRateHistoryService = exchangeRateHistoryService;
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task<bool> Consume(FillExchangeRateCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        string defaultCurrencyCode = _settings.DefaultCurrencyCode;
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var allExchangeRate =  await _exchangeRateClient.GetExchangeRates(defaultCurrencyCode, cancellationToken).ConfigureAwait(false);
        IList<Models.Currency> allCurrencies = await _currencyService.GetAll(currentUser, cancellationToken).ConfigureAwait(false);
        var baseCurrency = allCurrencies.FirstOrDefault(j => j.Code == defaultCurrencyCode);
        foreach (KeyValuePair<string, decimal> pair in allExchangeRate)
        {
            if (pair.Key != defaultCurrencyCode)
            {
                Models.Currency destinationCurrency = allCurrencies.FirstOrDefault(j => j.Code == pair.Key);
                if (destinationCurrency != null)
                {
                    

                    ExchangeRateHistoryCreateUpdateByDateDto createHistory = new ExchangeRateHistoryCreateUpdateByDateDto();
                    createHistory.FromCurrencyId = baseCurrency.Id;
                    createHistory.ToCurrencyId = destinationCurrency.Id;
                    createHistory.TargetDate= DateTime.Now;
                    createHistory.Rate = pair.Value;

                    await _exchangeRateHistoryService.CreateOrUpdateByDate(createHistory, currentUser, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    _logger.LogWarning($"Currency Code {pair.Key} not found");
                }
            }
        }

        return true;
    }
}