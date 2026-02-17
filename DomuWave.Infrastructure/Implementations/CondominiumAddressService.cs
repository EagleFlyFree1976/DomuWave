using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;
using DomuWave.Common;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class CondominiumAddressService : BaseService<CondominiumAddress, int>, ICondominiumAddressService
    {
        public CondominiumAddressService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "CondominiumAddresses";

        public async Task<CondominiumAddress> GetByCondominiumIdAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<CondominiumAddress>()
                .Where(a => a.CondominiumId == condominiumId && !a.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
