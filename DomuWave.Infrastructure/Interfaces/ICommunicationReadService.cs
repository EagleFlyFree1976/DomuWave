using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICommunicationReadService : IBaseService<CommunicationRead, int>
    {
        Task<IList<CommunicationRead>> GetByCommunicationIdAsync(int communicationId);
        Task<bool> MarkAsReadAsync(int communicationId, long userId);
        Task<int> GetUnreadCountAsync(long userId);
    }
}
