using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface ICurrencyService : IGenericService<Currency, int, CurrencyCreateUpdateDto, CurrencyCreateUpdateDto>, IService
{
    Task<IList<Currency>> Find(string q, IUser currentUser, CancellationToken cancellationToken);

    Task<Currency> GetDefault(IUser currentUser, CancellationToken cancellationToken);
    Task<ConvertResult> ConvertTo(decimal amount, Currency sourceCurrency, Currency destCurrency, DateTime targetDate,
        IUser currentUser,
        CancellationToken cancellationToken);
}