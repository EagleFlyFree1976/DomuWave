using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumCadastralDataService : IBaseService<CondominiumCadastralData, int>
    {
        Task<CondominiumCadastralData> GetByCondominiumIdAsync(int condominiumId);
    }
}
