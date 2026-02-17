using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;
using DomuWave.Common;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class CondominiumCadastralDataService : BaseService<CondominiumCadastralData, int>, ICondominiumCadastralDataService
    {
        public CondominiumCadastralDataService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "CondominiumCadastralData";

        public async Task<CondominiumCadastralData> GetByCondominiumIdAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<CondominiumCadastralData>()
                .Where(c => c.CondominiumId == condominiumId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
