using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IDocumentAccessService : IBaseService<DocumentAccess, long>
    {
        Task<IList<DocumentAccess>> GetByDocumentIdAsync(int documentId);
        Task<IList<DocumentAccess>> GetByUserIdAsync(long userId);
        Task<bool> LogAccessAsync(int documentId, long userId, string accessType, string ipAddress);
    }
}
