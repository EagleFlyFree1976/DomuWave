using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumAddressService : IBaseService<CondominiumAddress, int>
    {
        Task<CondominiumAddress> GetByCondominiumIdAsync(int condominiumId);
    }
}
