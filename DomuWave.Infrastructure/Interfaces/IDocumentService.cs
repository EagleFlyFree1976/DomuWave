using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IDocumentService : IBaseService<Document, int>
    {
        Task<IList<Document>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<Document>> GetByCategoryAsync(int condominiumId, string category);
        Task<IList<Document>> GetVisibleToOwnersAsync(int condominiumId);
        Task<IList<Document>> SearchDocumentsAsync(int condominiumId, string searchTerm);
        Task<IList<Document>> GetRecentDocumentsAsync(int condominiumId, int count);
    }
}
