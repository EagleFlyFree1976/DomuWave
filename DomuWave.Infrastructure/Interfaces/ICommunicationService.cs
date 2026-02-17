using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICommunicationService : IBaseService<Communication, int>
    {
        Task<IList<Communication>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<Communication>> GetVisibleCommunicationsAsync(int condominiumId);
        Task<IList<Communication>> GetByTypeAsync(int condominiumId, string communicationType);
        Task<IList<Communication>> GetByPriorityAsync(int condominiumId, string priority);
        Task<IList<Communication>> GetUnreadByUserAsync(int condominiumId, long userId);
        Task<bool> PublishCommunicationAsync(int communicationId, long userId);
    }
}
