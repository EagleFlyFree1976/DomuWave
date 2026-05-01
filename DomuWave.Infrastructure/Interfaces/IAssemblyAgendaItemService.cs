using CPQ.Core.Memberships;
using CPQ.Core.Services;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IAssemblyAgendaItemService : IBaseService<AssemblyAgendaItem, int>
{
    Task<IList<AssemblyAgendaItem>> GetByAssemblyIdAsync(int assemblyId, IUser currentUser, CancellationToken cancellationToken);
}
