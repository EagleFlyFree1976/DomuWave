using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IFaultService : IBaseService<Fault, int>
{
    Task<IList<Fault>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken ct);
}

public interface IFaultMessageService : IBaseService<FaultMessage, int>
{
    Task<IList<FaultMessage>> GetByFaultAsync(int faultId, IUser currentUser, CancellationToken ct);
}
