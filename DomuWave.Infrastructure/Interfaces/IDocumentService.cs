using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IDocumentService : IBaseService<Document, int>
    {
        Task<IList<Document>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Document>> GetByCategoryAsync(int condominiumId, string category, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Document>> GetVisibleToOwnersAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Document>> SearchDocumentsAsync(int condominiumId, string searchTerm, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Document>> GetRecentDocumentsAsync(int condominiumId, int count, IUser currentUser, CancellationToken cancellationToken);
    }
}
