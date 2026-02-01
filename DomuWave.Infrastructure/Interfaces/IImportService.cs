using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Import;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface IImportService : IService
{
    Task<Import> GetByIdAsync(long importId, CancellationToken cancellationToken);
    Task<Import> SaveAsync(Import import, CancellationToken cancellationToken);
    Task DeleteAsync(Import import, CancellationToken cancellationToken);
    Task<IList<Import>> FindByBookIdAsync(int bookId, CancellationToken cancellationToken);
    
}