using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class TransactionStatusService : BaseService, ITransactionStatusService
{
 
    
    public override string CacheRegion
    {
        get { return "TransactionStatus"; }
    }


    public TransactionStatusService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
    {
    }

    public async Task<TransactionStatus> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        string key = "TransactionStatus_" + id;
        var data = _cache.Get<TransactionStatus>(key);
        if (data == null)
        {
            data = await session.GetAsync<TransactionStatus>(id, cancellationToken).ConfigureAwait(false);
            _cache.Set(CacheRegion, key, data, 10);
        }
        return data;
    }

    public async Task<TransactionStatus> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        string key = "TransactionStatusByCode_" + code;
        var data = _cache.Get<TransactionStatus>(key);
        if (data == null)
        {
            data = await session.Query<TransactionStatus>().Where(l => l.Code == code && !l.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
            _cache.Set(CacheRegion, key, data, 10);
        }
        return data;

      

    }

    public async Task<IList<TransactionStatus>> FindAll(CancellationToken cancellationToken)
    {
        string key = "TransactionStatusList";
        var data = _cache.Get<IList<TransactionStatus>>(key);
        if (data == null)
        {
            data = await session.Query<TransactionStatus>().ToListAsync(cancellationToken).ConfigureAwait(false);
            _cache.Set("TransactionStatus",key, data, 10);
        }
        return data;

        
    }
}