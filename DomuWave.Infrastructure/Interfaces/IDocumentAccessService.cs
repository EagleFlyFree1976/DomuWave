using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IDocumentAccessService : IBaseService<DocumentAccess, long>
    {
        Task<IList<DocumentAccess>> GetByDocumentIdAsync(int documentId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<DocumentAccess>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> LogAccessAsync(int documentId, long userId, string accessType, string ipAddress, IUser currentUser, CancellationToken cancellationToken);
    }
}
