using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IMessageService : IBaseService<Message, long>
    {
        Task<IList<Message>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Message>> GetBySenderIdAsync(long senderId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Message>> GetByRecipientIdAsync(long recipientId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Message>> GetConversationAsync(long userId1, long userId2, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Message>> GetUnreadMessagesAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> MarkAsReadAsync(long messageId, IUser currentUser, CancellationToken cancellationToken);
        Task<int> GetUnreadCountAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
    }
}
