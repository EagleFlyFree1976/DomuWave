using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface IGenericService<T, TKey, TDto, TUpdateDto>
{
    Task<T> GetById(TKey itemId, IUser currentUser, CancellationToken cancellationToken);
    Task<IList<T>> GetAll(IUser currentUser, CancellationToken cancellationToken);

    Task Delete(TKey entityId, IUser currentUser, CancellationToken cancellationToken);

    Task<T> Create(TDto dto, IUser currentUser, CancellationToken cancellationToken);
    Task<T> Update(TKey entityId, TUpdateDto updateDto, IUser currentUser, CancellationToken cancellationToken);



}