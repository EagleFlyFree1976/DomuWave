using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IDynamicFileService : IService<DynamicFile, int>
{
    Task<IList<DynamicFile>> GetByEntityAsync(string entityName, int entityId, IUser currentUser, CancellationToken ct);
    Task<IList<DynamicFile>> GetByEntityFullNameAsync(string entityFullName, int entityId, IUser currentUser, CancellationToken ct);
    Task<DynamicFileContent?> GetContentAsync(int fileId, IUser currentUser, CancellationToken ct);
}
