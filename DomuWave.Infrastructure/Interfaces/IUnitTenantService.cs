using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IUnitTenantService : IBaseService<UnitTenant, int>
    {
        Task<IList<UnitTenant>> GetByUnitIdAsync(int unitId);
        Task<UnitTenant> GetActiveByUnitIdAsync(int unitId);
        Task<IList<UnitTenant>> GetExpiringLeasesAsync(Guid tenantId, int daysAhead);
    }
}
