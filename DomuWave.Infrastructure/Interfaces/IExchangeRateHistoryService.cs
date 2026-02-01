using DomuWave.Services.Helper;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface IExchangeRateHistoryService : IGenericService<ExchangeRateHistory, long, ExchangeRateHistoryCreateUpdateDto,
    ExchangeRateHistoryCreateUpdateDto>
{
    Task<ExchangeRateHistory> CreateOrUpdateByDate(ExchangeRateHistoryCreateUpdateByDateDto item, IUser currentUser, CancellationToken cancellationToken);


    Task<PagedResult<ExchangeRateHistory>> GetByDateAsync(DateTime? targetDate, int? toCurrencyId, int? pageNumber, int? pageSize, string sortBy, bool asc, IUser currentUser, CancellationToken cancellationToken);
    Task<ExchangeRateHistory> GetExchangeRateAsync(DateTime targetDate, int fromCurrencyId, int toCurrencyId,
        bool exactlyMode,
        IUser currentUser, CancellationToken cancellationToken);
}