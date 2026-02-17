using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IMessageService : IBaseService<Message, long>
    {
        Task<IList<Message>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<Message>> GetBySenderIdAsync(long senderId);
        Task<IList<Message>> GetByRecipientIdAsync(long recipientId);
        Task<IList<Message>> GetConversationAsync(long userId1, long userId2);
        Task<IList<Message>> GetUnreadMessagesAsync(long userId);
        Task<bool> MarkAsReadAsync(long messageId);
        Task<int> GetUnreadCountAsync(long userId);
    }
}
