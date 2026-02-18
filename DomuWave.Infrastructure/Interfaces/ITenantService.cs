using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ITenantService : IBaseService<Tenant, Guid>
    {
        Task<Tenant> GetByCodeAsync(string code, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Tenant>> GetActiveTenantsAsync(IUser currentUser, CancellationToken cancellationToken);
    }
}
