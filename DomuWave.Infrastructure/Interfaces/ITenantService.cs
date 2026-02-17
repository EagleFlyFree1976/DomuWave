using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ITenantService : IBaseService<Tenant, Guid>
    {
        Task<Tenant> GetByCodeAsync(string code);
        Task<IList<Tenant>> GetActiveTenantsAsync();
    }
}
