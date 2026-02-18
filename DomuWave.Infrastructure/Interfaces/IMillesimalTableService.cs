using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IMillesimalTableService : IBaseService<MillesimalTable, int>
    {
        Task<IList<MillesimalTable>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<MillesimalTable> GetByCodeAsync(int condominiumId, string code, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<MillesimalTable>> GetActiveTablesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
    }
}
