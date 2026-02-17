using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;
using DomuWave.Common;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class UnitMillesimalService : BaseService<UnitMillesimal, int>, IUnitMillesimalService
    {
        public UnitMillesimalService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "UnitMillesimals";

        public async Task<IList<UnitMillesimal>> GetByMillesimalTableIdAsync(int millesimalTableId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitMillesimal>()
                .Where(m => m.MillesimalTableId == millesimalTableId && !m.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitMillesimal>> GetByUnitIdAsync(int unitId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitMillesimal>()
                .Where(m => m.UnitId == unitId && !m.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitMillesimal> GetByTableAndUnitAsync(int millesimalTableId, int unitId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitMillesimal>()
                .Where(m => m.MillesimalTableId == millesimalTableId 
                         && m.UnitId == unitId 
                         && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalMillesimalAsync(int millesimalTableId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitMillesimal>()
                .Where(m => m.MillesimalTableId == millesimalTableId && !m.IsDeleted)
                .SumAsync(m => m.Millesimal, cancellationToken);
        }

        public async Task<bool> ValidateTotalMillesimalAsync(int millesimalTableId, decimal expectedTotal, CancellationToken cancellationToken = default)
        {
            var total = await GetTotalMillesimalAsync(millesimalTableId, cancellationToken);
            return total == expectedTotal;
        }
    }
}
