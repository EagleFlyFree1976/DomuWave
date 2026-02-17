using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Persistence.SessionFactories;
using NHibernate.Linq;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class RealEstateUnitService: BaseService, IRealEstateUnitService
    {
        public RealEstateUnitService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "RealEstateUnits";

        public async Task<IList<RealEstateUnit>> GetByCondominiumIdAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<RealEstateUnit>()
                .Where(u => u.CondominiumId == condominiumId && !u.IsDeleted)
                .OrderBy(u => u.Staircase)
                .ThenBy(u => u.Floor)
                .ThenBy(u => u.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetByStaircaseAsync(int condominiumId, string staircase, CancellationToken cancellationToken = default)
        {
            return await session.Query<RealEstateUnit>()
                .Where(u => u.CondominiumId == condominiumId 
                         && u.Staircase == staircase 
                         && !u.IsDeleted)
                .OrderBy(u => u.Floor)
                .ThenBy(u => u.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetByFloorAsync(int condominiumId, int floor, CancellationToken cancellationToken = default)
        {
            return await session.Query<RealEstateUnit>()
                .Where(u => u.CondominiumId == condominiumId 
                         && u.Floor == floor 
                         && !u.IsDeleted)
                .OrderBy(u => u.Staircase)
                .ThenBy(u => u.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetByTypeAsync(int condominiumId, string unitType, CancellationToken cancellationToken = default)
        {
            return await session.Query<RealEstateUnit>()
                .Where(u => u.CondominiumId == condominiumId 
                         && u.UnitType == unitType 
                         && !u.IsDeleted)
                .OrderBy(u => u.Staircase)
                .ThenBy(u => u.Floor)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalUnitsCountAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<RealEstateUnit>()
                .CountAsync(u => u.CondominiumId == condominiumId && !u.IsDeleted, cancellationToken);
        }
    }
}
