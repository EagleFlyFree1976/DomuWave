using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICommunicationReadService : IBaseService<CommunicationRead, int>
    {
        Task<IList<CommunicationRead>> GetByCommunicationIdAsync(int communicationId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> MarkAsReadAsync(int communicationId, long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<int> GetUnreadCountAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
    }
}
