using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICommunicationService : IBaseService<Communication, int>
    {

        Task<IList<Communication>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Communication>> GetVisibleCommunicationsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Communication>> GetByTypeAsync(int condominiumId, string communicationType, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Communication>> GetByPriorityAsync(int condominiumId, string priority, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Communication>> GetUnreadByUserAsync(int condominiumId, long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> PublishCommunicationAsync(int communicationId, long userId, IUser currentUser, CancellationToken cancellationToken);
    }
}
