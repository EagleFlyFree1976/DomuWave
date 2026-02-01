using System.Collections.Generic;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using DomuWave.Services.Settings;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using Microsoft.Extensions.Options;
using NHibernate.Engine.Query;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class CurrencyService : BaseService, ICurrencyService
{
    private DomuWaveSettings _settings;

    public CurrencyService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, IOptions<DomuWaveSettings> settings) : base(sessionFactoryProvider, cache)
    {
        _settings = settings.Value;
    }

    public override string CacheRegion
    {
        get { return "Currency"; }

    }



    public async Task<Currency> GetById(int itemId, IUser currentUser, CancellationToken cancellationToken)
    {
        
        return await session.GetAsync<Currency>(itemId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<Currency>> GetAll(IUser currentUser, CancellationToken cancellationToken)
    {
        string key = "GetAll";
        IList<Currency> currencies = _cache.Get<IList<Currency>>(key);
        if (currencies == null)
        {
            currencies = await session.Query<Currency>().GetQueryable<Currency, int>()
                        .ToListAsync(cancellationToken).ConfigureAwait(false);

            _cache.Set(CacheRegion,key, currencies, _settings.CacheTimeouts.Currency);
        }
        return currencies;
    }

    public async Task<IList<Currency>> Find(string q, IUser currentUser, CancellationToken cancellationToken)
    {
        var allCurrencies = await GetAll(currentUser, cancellationToken).ConfigureAwait(false);
    
        if (!string.IsNullOrEmpty(q))
            return allCurrencies.Where(k => k.Name.ToLower().Contains(q.ToLower()) || k.Code.ToLower().Contains(q.ToLower()) || k.Symbol.ToLower().Contains(q.ToLower())).ToList();

        return allCurrencies;
    }

    public Task Delete(int entityId, IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task Validate(int? entityId, string name, string code, int decimalDigits, string symbol, IUser currentUser,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidatorException("Specificare il nome");
        }
        IQueryable<Currency> accounts = session.Query<Currency>().GetQueryable<Currency, int>();


        if (entityId.HasValue)
        {
            accounts = accounts.Where(j => j.Id != entityId);
        }
        var existsByName = await accounts.Where(k => k.Name == name).AnyAsync(cancellationToken);
        if (existsByName)
        {
            throw new ValidatorException($"Esiste già una valuta con questo nome");
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new ValidatorException("Specificare il codice");
        }

        if (code.Trim().Length > 3)
        {
            throw new ValidatorException("Il codice deve essere lungo al massimo 3 caratteri");

        }
        var existsByCode = await accounts.Where(j => j.Code == code).AnyAsync(cancellationToken);
        if (existsByCode)
        {
            throw new ValidatorException($"Esiste già una valuta con questo codice");
        }

        if (string.IsNullOrEmpty(symbol))
        {
            throw new ValidatorException("Specificare il simbolo");
        }
        var existsBySymbol = await accounts.Where(j => j.Symbol == symbol).AnyAsync(cancellationToken);
        if (existsBySymbol)
        {
            throw new ValidatorException($"Esiste già una valuta con questo simbolo");
        }

    }


    public async Task<Currency> Create(CurrencyCreateUpdateDto createDto, IUser currentUser, CancellationToken cancellationToken)
    {
        await Validate(null, createDto.Name, createDto.Code, createDto.DecimalDigits, createDto.Symbol, currentUser, cancellationToken).ConfigureAwait(false);



        Currency newCurrency = new Currency()
        {
            Name = createDto.Name,
            Code = createDto.Code,
            Symbol = createDto.Symbol,
            DecimalDigits = createDto.DecimalDigits,
            IsEnabled = true
        };

        newCurrency.Trace(currentUser);

        await session.SaveAsync(newCurrency, cancellationToken).ConfigureAwait(false);

        return newCurrency;
    }

    public async Task<Currency> Update(int entityId, CurrencyCreateUpdateDto updateDto, IUser currentUser, CancellationToken cancellationToken)
    {
        Currency editCurrency = await session.GetAsync<Currency>(entityId, cancellationToken).ConfigureAwait(false);

        if (editCurrency == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }
        await Validate(entityId, updateDto.Name, updateDto.Code, updateDto.DecimalDigits, updateDto.Symbol, currentUser, cancellationToken).ConfigureAwait(false);

        editCurrency.Name = updateDto.Name;
        editCurrency.Code = updateDto.Code;
        editCurrency.Symbol = updateDto.Symbol;
        editCurrency.DecimalDigits = updateDto.DecimalDigits;
        editCurrency.IsEnabled = updateDto.IsEnabled;
        editCurrency.Trace(currentUser);
        await session.SaveAsync(editCurrency, cancellationToken).ConfigureAwait(false);
        return editCurrency;
    }


    public async Task<Currency> GetDefault(IUser currentUser, CancellationToken cancellationToken)
    {
        Currency defaultCurrency = await session.Query<Currency>()
            .Where(k => !k.IsDeleted && k.Code == _settings.DefaultCurrencyCode)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);



        return defaultCurrency;
    }

    private async Task<decimal> GetRate(Currency sourceCurrency, Currency destCurrency, Currency defaultCurrency, IList<ExchangeRateHistory> allExchangeRates, CancellationToken cancellationToken)
    {


        if (sourceCurrency.Id == destCurrency.Id)
            return 1;
        if (destCurrency.Id == defaultCurrency.Id)
            return 1 / await GetRate(destCurrency, sourceCurrency, defaultCurrency, allExchangeRates, cancellationToken).ConfigureAwait(false);

        ExchangeRateHistory exchangeRate = allExchangeRates.FirstOrDefault(k => k.FromCurrency.Id == defaultCurrency.Id && k.ToCurrency.Id == destCurrency.Id);
        if (exchangeRate == null)
            throw new ValidatorException($"Impossibile convertire da {sourceCurrency.Name} a {destCurrency.Name}");



        if (sourceCurrency.Code == destCurrency.Code || sourceCurrency.Id == defaultCurrency.Id)
            return exchangeRate.Rate;
        else
        {


            ExchangeRateHistory sourceCurrencyER = allExchangeRates.FirstOrDefault(k => k.FromCurrency.Id == defaultCurrency.Id && k.ToCurrency.Id == sourceCurrency.Id);
            ExchangeRateHistory destnationExchangeRateHistory = allExchangeRates.FirstOrDefault(k => k.FromCurrency.Id == defaultCurrency.Id && k.ToCurrency.Id == destCurrency.Id);
            if (sourceCurrencyER == null)
                throw new ValidatorException($"Impossibile converire nella valuta di destinazione");

            return destnationExchangeRateHistory.Rate / sourceCurrencyER.Rate;
        }

    }

    public async Task<ConvertResult> ConvertTo(decimal amount, Currency sourceCurrency, Currency destCurrency,
        DateTime targetDate, IUser currentUser, CancellationToken cancellationToken)
    {

        if (amount == 0)
            return new ConvertResult() { Amount = 0, Rate = 0 };

        if (amount < 0)
            amount = -1 * amount;

        decimal convertedValue = 0;

        //var currencyRange = targetDate.ToCurrencyRange();

        if (sourceCurrency.Id == destCurrency.Id)
            return new ConvertResult() { Amount = amount, Rate = 1 };

        Currency defaultCurrency = await GetDefault(currentUser, cancellationToken).ConfigureAwait(false);

        var query = session.Query<ExchangeRateHistory>();
        var allExchangeRates = await query
            .Where(x => x.ValidFrom <= targetDate)
            .OrderByDescending(x =>
                x.ValidTo == null || x.ValidTo >= targetDate ? targetDate : x.ValidTo)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        //var allExchangeRates = await session.Query<ExchangeRateHistory>()
        //        .Where(k=>!k.IsDeleted)
        //        .Where(k=>k.ValidFrom == currencyRange.from && (!k.ValidTo.HasValue || k.ValidTo.Value <= currencyRange.to)).ToListAsync(cancellationToken)
        //        .ConfigureAwait(false);

        decimal rate = await GetRate(sourceCurrency, destCurrency, defaultCurrency, allExchangeRates, cancellationToken)
            .ConfigureAwait(false);

        convertedValue = amount * rate;
        return new ConvertResult() { Amount = convertedValue, Rate = rate };
    }
}