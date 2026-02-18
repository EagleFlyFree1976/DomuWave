using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IUnitTenantService : IBaseService<UnitTenant, int>
    {
        Task<IList<UnitTenant>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken);
        Task<UnitTenant> GetActiveByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<UnitTenant>> GetExpiringLeasesAsync(Guid tenantId, int daysAhead, IUser currentUser, CancellationToken cancellationToken);
    }
}
