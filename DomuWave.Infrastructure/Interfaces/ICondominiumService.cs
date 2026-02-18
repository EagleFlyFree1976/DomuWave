using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumService : IBaseService<Condominium, int>
    {
        Task<Condominium> GetByCodeAsync(Guid tenantId, string code, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Condominium>> GetActiveCondominiumsAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Condominium>> GetCondominiumsWithUpcomingAssemblyAsync(Guid tenantId, int daysAhead, IUser currentUser, CancellationToken cancellationToken);
    }
}
