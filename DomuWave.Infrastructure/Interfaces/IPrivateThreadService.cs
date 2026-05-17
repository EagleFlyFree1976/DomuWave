using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IPrivateThreadService : IBaseService<PrivateThread, int>
{
    Task<IList<PrivateThread>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken ct);
    Task<PrivateThread?> GetByCondominiumAndUserAsync(int condominiumId, long condominioUserId, IUser currentUser, CancellationToken ct);
}

public interface IPrivateMessageService : IBaseService<PrivateMessage, int>
{
    Task<IList<PrivateMessage>> GetByThreadAsync(int threadId, IUser currentUser, CancellationToken ct);
}
