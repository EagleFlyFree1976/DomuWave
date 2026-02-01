using DomuWave.Services.Extensions;
using DomuWave.Services.Helper;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using NHibernate.Linq;
using NHibernate.Type;

namespace DomuWave.Services.Implementations;

public class ExchangeRateHistoryService : BaseService, IExchangeRateHistoryService
{
    public override string CacheRegion
    {
        get { return "ExchangeRateHistory"; }
    }
    public ExchangeRateHistoryService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
    {
    }


    public async Task<ExchangeRateHistory> GetById(long itemId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.GetAsync<ExchangeRateHistory>(itemId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<ExchangeRateHistory>> GetAll(IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<ExchangeRateHistory>().GetQueryable<ExchangeRateHistory,long>().ToListAsync(cancellationToken);
    }

   
    public async Task Delete(long entityId, IUser currentUser, CancellationToken cancellationToken)
    {
        var exchangeRate = await GetById(entityId, currentUser, cancellationToken).ConfigureAwait(false);
        if (exchangeRate == null)
        {
            throw new NotFoundException("Tasso di cambio non trovato.");
        }

        exchangeRate.IsDeleted = true;
        exchangeRate.Trace(currentUser);
        await session.SaveAsync(exchangeRate, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(Currency fromCurrency, Currency toCurrency)> Validate(long? entityId, int fromCurrenctId, int toCurrencyId, decimal rate,DateTime from, DateTime? to, IUser currentUser,
        CancellationToken cancellationToken)
    {

        if (to.HasValue && from > to)
        {
            throw new ValidatorException($"La data finale deve essere superiore dalla data iniziale");
        }


        IQueryable<ExchangeRateHistory> accounts = session.Query<ExchangeRateHistory>().GetQueryable<ExchangeRateHistory, long>();
                
        var existsItem = await session.Query<ExchangeRateHistory>().Where(k =>
            k.FromCurrency.Id == fromCurrenctId && k.ToCurrency.Id == toCurrencyId &&
            (entityId == null || k.Id != entityId) &&
            (from == k.ValidFrom && (!k.ValidTo.HasValue || to== k.ValidTo.Value)) &&
            !k.IsDeleted
        ).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (existsItem != null && (!entityId.HasValue || existsItem.Id != entityId.Value))
        {
            throw new ValidatorException("Esiste già un tasso di cambio per questa coppia di valute con le stesse date di validità.");
        }
        // todo


        var fromCurrency = await session.GetAsync<Currency>(fromCurrenctId, cancellationToken)
            .ConfigureAwait(false);
        var toCurrency = await session.GetAsync<Currency>(toCurrencyId, cancellationToken)
            .ConfigureAwait(false);


        if (fromCurrency == null)
                throw new NotFoundException($"Valuta con ID {fromCurrenctId} non trovata.");
        if (toCurrency == null)
                throw new NotFoundException($"Valuta con ID {toCurrencyId} non trovata.");

        return (fromCurrency, toCurrency);
    }


    public async Task<ExchangeRateHistory> Create(ExchangeRateHistoryCreateUpdateDto createDto, IUser currentUser, CancellationToken cancellationToken)
    {
        var result = await Validate(null, createDto.FromCurrencyId, createDto.ToCurrencyId, createDto.Rate, createDto.ValidFrom, createDto.ValidTo, currentUser, cancellationToken).ConfigureAwait(false);

        ExchangeRateHistory newExchangeRateHistory = new ExchangeRateHistory()
        {
            FromCurrency = result.fromCurrency,
            ToCurrency = result.toCurrency,
            Rate = createDto.Rate,
            ValidFrom = createDto.ValidFrom,
            ValidTo = createDto.ValidTo,
            IsEnabled = true
        };

        newExchangeRateHistory.Trace(currentUser);

        await session.SaveAsync(newExchangeRateHistory, cancellationToken).ConfigureAwait(false);

        return newExchangeRateHistory;
    }


    public async Task<ExchangeRateHistory> CreateOrUpdateByDate(ExchangeRateHistoryCreateUpdateByDateDto item,
        IUser currentUser, CancellationToken cancellationToken)
    {
        DateTime _validFrom = item.TargetDate.Date;
        DateTime _validTo = _validFrom.Date.AddDays(1).AddSeconds(-1);

        var existsItem = await session.Query<ExchangeRateHistory>().Where(k =>
            k.FromCurrency.Id == item.FromCurrencyId && k.ToCurrency.Id == item.ToCurrencyId &&
            item.TargetDate >= k.ValidFrom
            && (!k.ValidTo.HasValue || item.TargetDate < k.ValidTo.Value)
            && k.IsEnabled
            && !k.IsDeleted
        ).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        ExchangeRateHistory _exchangeRateHistory = null;
        if (existsItem != null)
        {
            _exchangeRateHistory  = await Update(existsItem.Id,
                new ExchangeRateHistoryCreateUpdateDto()
                {
                    FromCurrencyId = item.FromCurrencyId, ToCurrencyId = item.ToCurrencyId, Rate = item.Rate,
                    ValidFrom = _validFrom, ValidTo = _validTo
                }, currentUser, cancellationToken);

        }
        else
        {
            _exchangeRateHistory = await Create(
                new ExchangeRateHistoryCreateUpdateDto()
                {
                    FromCurrencyId = item.FromCurrencyId,
                    ToCurrencyId = item.ToCurrencyId,
                    Rate = item.Rate,
                    ValidFrom = _validFrom,
                    ValidTo = _validTo
                }, currentUser, cancellationToken);
        }

        return _exchangeRateHistory;
    }


    public async Task<ExchangeRateHistory> Update(long entityId, ExchangeRateHistoryCreateUpdateDto updateDto, IUser currentUser, CancellationToken cancellationToken)
    {
        ExchangeRateHistory editExchangeRateHistory = await session.GetAsync<ExchangeRateHistory>(entityId, cancellationToken).ConfigureAwait(false);

        if (editExchangeRateHistory == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }
        await Validate(entityId, updateDto.FromCurrencyId, updateDto.ToCurrencyId, updateDto.Rate, updateDto.ValidFrom, updateDto.ValidTo, currentUser, cancellationToken).ConfigureAwait(false);

     

        editExchangeRateHistory.Rate = updateDto.Rate;
        editExchangeRateHistory.ValidFrom = updateDto.ValidFrom;
        editExchangeRateHistory.ValidTo= updateDto.ValidTo;
   

        editExchangeRateHistory.Trace(currentUser);

        await session.SaveAsync(editExchangeRateHistory, cancellationToken).ConfigureAwait(false);

        return editExchangeRateHistory;
    }
    private async Task<ExchangeRateHistory?> GetExchangeRateByCurrenciesAndDateAsync(int fromCurrencyId,
        int toCurrencyId, DateTime targetDate, bool exactlyMode, CancellationToken cancellationToken)
    {
        if (fromCurrencyId == toCurrencyId)
            return null;

        var item = await session.Query<ExchangeRateHistory>().Where(k =>
            k.FromCurrency.Id == fromCurrencyId &&
            k.ToCurrency.Id == toCurrencyId &&
            targetDate >= k.ValidFrom &&
            (!k.ValidTo.HasValue || targetDate < k.ValidTo.Value) &&
            k.IsEnabled &&
            !k.IsDeleted
        ).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (item != null || exactlyMode )
        {
            return item;
        }
        if (item == null && !exactlyMode)
        {
            // prendo l'ultimo tasso di cambio disponibile
            item = await session.Query<ExchangeRateHistory>().Where(k =>
                k.FromCurrency.Id == fromCurrencyId &&
                k.ToCurrency.Id == toCurrencyId &&
                targetDate >= k.ValidFrom &&
                (!k.ValidTo.HasValue || k.ValidTo.Value < targetDate) &&
                k.IsEnabled && !k.IsDeleted
            ).OrderByDescending(k => k.ValidFrom).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        return item;
    }
    public async Task<ExchangeRateHistory> GetExchangeRateAsync(DateTime targetDate, int fromCurrencyId,
        int toCurrencyId, bool exactlyMode, IUser currentUser,
        CancellationToken cancellationToken)
    {
        
        Currency defaultCurrency = await session.Query<Currency>().Where(k => k.IsDefault && !k.IsDeleted).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (defaultCurrency == null)
        {
            throw new ValidatorException("Non è stata definita una valuta di default.");
        }
        if ((fromCurrencyId == default(int) && toCurrencyId == defaultCurrency.Id) || (fromCurrencyId != default(int) && fromCurrencyId == toCurrencyId))
        {
            // stessa valuta
            ExchangeRateHistory virtualExchangeRate = new ExchangeRateHistory()
            {
                FromCurrency = await session.GetAsync<Currency>(fromCurrencyId, cancellationToken).ConfigureAwait(false),
                ToCurrency = await session.GetAsync<Currency>(toCurrencyId, cancellationToken).ConfigureAwait(false),
                Rate = 1,
                ValidFrom = targetDate,
                ValidTo = null,
                IsEnabled = true
            };
            return virtualExchangeRate;
        }
        if (fromCurrencyId == defaultCurrency.Id)
        {
            var item = await GetExchangeRateByCurrenciesAndDateAsync(fromCurrencyId, toCurrencyId, targetDate, exactlyMode, cancellationToken).ConfigureAwait(false);
            return item;
        }
        else
        {

            var fromCurrency = await session.GetAsync<Currency>(fromCurrencyId, cancellationToken).ConfigureAwait(false);
            var toCurrency = await session.GetAsync<Currency>(toCurrencyId, cancellationToken).ConfigureAwait(false);

            // ricavo il tasso di cambio della valuta di partenza verso la valuta di default 
            ExchangeRateHistory? exgFrom = null;
            if (fromCurrencyId != default(int))
                exgFrom = await GetExchangeRateByCurrenciesAndDateAsync(defaultCurrency.Id,fromCurrencyId,  targetDate, exactlyMode, cancellationToken).ConfigureAwait(false);

            // ricavo il tasso di cambio della valuta di destinazione verso la valuta di default 
            var exgTo = await GetExchangeRateByCurrenciesAndDateAsync(defaultCurrency.Id, toCurrencyId, targetDate, exactlyMode, cancellationToken).ConfigureAwait(false);
            if (exgTo == null && toCurrencyId != default(int) && toCurrencyId != defaultCurrency.Id)
            {
                throw new NotFoundException($"Non è stato trovato un tasso di cambio per la valuta");
            }
            var toRate = exgTo != null ? exgTo.Rate : 1;
            var fromRate = exgFrom != null ? exgFrom.Rate : 1;
            var rateFromTo = toRate / fromRate;
            ExchangeRateHistory virtualExchangeRate = new ExchangeRateHistory()
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = rateFromTo,
                ValidFrom = exgTo != null ? exgTo.ValidFrom : exgFrom.ValidFrom,
                ValidTo = exgTo != null ? exgTo.ValidTo : exgFrom.ValidTo,
                
                IsEnabled = true
            };
            return virtualExchangeRate;
        }



    }

    public async Task<PagedResult<ExchangeRateHistory>> GetByDateAsync(DateTime? targetDate, int? toCurrencyId,
        int? pageNumber, int? pageSize, string sortBy, bool asc, IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<ExchangeRateHistory>().GetQueryable<ExchangeRateHistory, long>();

        query = query.Where(i=>!i.IsDeleted);
        if (targetDate.HasValue)
        {
            query = query.Where(i => i.ValidFrom <= targetDate && (!i.ValidTo.HasValue || i.ValidTo > targetDate));
        }
        if (toCurrencyId.HasValue)
        {
            query = query.Where(i => i.ToCurrency.Id == toCurrencyId.Value);
        }

        if (string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDescending(k => k.ValidFrom);
        }
        else
        {
            switch (sortBy)
            {
                case "ValidFrom":
                    query = asc ? query.OrderBy(k => k.ValidFrom)
                        : query.OrderByDescending(k => k.ValidFrom);
                    break;

                case "ValidTo":
                    query = asc ? query.OrderBy(k => k.ValidTo)
                        : query.OrderByDescending(k => k.ValidTo);
                    break;

                case "Rate":
                    query = asc ? query.OrderBy(k => k.Rate)
                        : query.OrderByDescending(k => k.Rate);
                    break;

                case "ToCurrency":
                    query = asc ? query.OrderBy(k => k.ToCurrency.Code)
                        : query.OrderByDescending(k => k.ToCurrency.Code);
                    break;

                default:
                    query = query.OrderByDescending(k => k.ValidFrom); // fallback sicuro
                    break;
            }
        }

        // Calcolo totale record prima della paginazione
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Default values se non passati
        var currentPage = pageNumber.GetValueOrDefault(1);
        var currentSize = pageSize.GetValueOrDefault(20);

        // Applico la paginazione
        var items = await query
            .Skip((currentPage - 1) * currentSize)
            .Take(currentSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<ExchangeRateHistory>
        {
            PageNumber = currentPage,
            PageSize = currentSize,
            TotalCount = totalCount,
            Items = items
        };



    }
}