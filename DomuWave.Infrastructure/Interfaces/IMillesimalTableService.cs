using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IMillesimalTableService : IBaseService<MillesimalTable, int>
    {
        Task<IList<MillesimalTable>> GetByCondominiumIdAsync(int condominiumId);
        Task<MillesimalTable> GetByCodeAsync(int condominiumId, string code);
        Task<IList<MillesimalTable>> GetActiveTablesAsync(int condominiumId);
    }
}
