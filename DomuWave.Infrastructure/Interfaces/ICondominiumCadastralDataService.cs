using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumCadastralDataService : IBaseService<CondominiumCadastralData, int>
    {
        Task<CondominiumCadastralData> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
    }
}
