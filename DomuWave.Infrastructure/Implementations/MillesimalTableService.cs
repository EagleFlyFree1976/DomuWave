using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Persistence.SessionFactories;
using NHibernate.Linq;
using DomuWave.Common;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class MillesimalTableService : BaseService<MillesimalTable, int>, IMillesimalTableService
    {
        public MillesimalTableService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "MillesimalTables";

        public async Task<IList<MillesimalTable>> GetByCondominiumIdAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<MillesimalTable>()
                .Where(m => m.CondominiumId == condominiumId && !m.IsDeleted)
                .OrderBy(m => m.Code)
                .ToListAsync(cancellationToken);
        }

        public async Task<MillesimalTable> GetByCodeAsync(int condominiumId, string code, CancellationToken cancellationToken = default)
        {
            return await session.Query<MillesimalTable>()
                .Where(m => m.CondominiumId == condominiumId && m.Code == code && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<MillesimalTable>> GetActiveTablesAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<MillesimalTable>()
                .Where(m => m.CondominiumId == condominiumId && m.IsActive && !m.IsDeleted)
                .OrderBy(m => m.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
