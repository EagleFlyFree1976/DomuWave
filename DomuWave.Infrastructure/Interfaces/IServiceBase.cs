using DomuWave.Services.Models;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface IServiceBase<T, T1, TCreateDto, TUpdateDto, TReadDto> : IService where T: BookEntity<T1> 
{

    Task<T> GetById(T1 itemId, long currentUserBookId, IUser currentUser, CancellationToken cancellationToken);
       
    Task<IList<T>> GetAll(IUser currentUser, long? bookId, CancellationToken cancellationToken);
    Task<T> Create(TCreateDto dto, long currentUserBookId, IUser currentUser, CancellationToken cancellationToken);
    Task<T> Update(T1 entityId, TUpdateDto updateDto, IUser currentUser,
        CancellationToken cancellationToken);
    Task Delete(T1 categoryId, long bookId, IUser currentUser, CancellationToken cancellationToken);
}