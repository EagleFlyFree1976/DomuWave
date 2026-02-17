using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IUnitMillesimalService : IBaseService<UnitMillesimal, int>
    {
        Task<IList<UnitMillesimal>> GetByMillesimalTableIdAsync(int millesimalTableId);
        Task<IList<UnitMillesimal>> GetByUnitIdAsync(int unitId);
        Task<UnitMillesimal> GetByTableAndUnitAsync(int millesimalTableId, int unitId);
        Task<decimal> GetTotalMillesimalAsync(int millesimalTableId);
        Task<bool> ValidateTotalMillesimalAsync(int millesimalTableId, decimal expectedTotal);
    }
}
