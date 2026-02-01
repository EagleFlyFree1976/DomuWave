using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Tag;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface ITagService : IServiceBase<Tag, long, TagCreateUpdateDto, TagCreateUpdateDto, TagReadDto>
{
    Task<Tag> GetOrCreateTag(string tagName, long bookId, IUser currentUser, CancellationToken cancellationToken);
    Task<Tag> GetByName(string tagName, long bookId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Tag>> FindAll(string q, long bookId, IUser currentUser, CancellationToken cancellationToken);
}