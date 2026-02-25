using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IUnitMillesimalService : IBaseService<UnitMillesimal, int>
    {
        Task<IList<UnitMillesimal>> GetByMillesimalTableIdAsync(int millesimalTableId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<UnitMillesimal>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken);
        Task<UnitMillesimal> GetByTableAndUnitAsync(int millesimalTableId, int unitId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalMillesimalAsync(int millesimalTableId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> ValidateTotalMillesimalAsync(int millesimalTableId, decimal expectedTotal, IUser currentUser, CancellationToken cancellationToken);
    }
}
