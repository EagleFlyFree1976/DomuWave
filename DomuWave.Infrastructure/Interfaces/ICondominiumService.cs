using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumService : IBaseService<Condominium, int>
    {
        Task<Condominium> GetByCodeAsync(Guid tenantId, string code);
        Task<IList<Condominium>> GetActiveCondominiumsAsync(Guid tenantId);
        Task<IList<Condominium>> GetCondominiumsWithUpcomingAssemblyAsync(Guid tenantId, int daysAhead);
    }
}
