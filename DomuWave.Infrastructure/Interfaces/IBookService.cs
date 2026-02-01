using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface IBookService : IGenericService<Book, long, BookDto, BookDto>
{
    Task<Book> GetSystem(IUser currentUser, CancellationToken cancellationToken);
    Task<Book> GetById(long itemId, CancellationToken cancellationToken);
    Task<Book> GetById(long itemId, IUser currentUser, CancellationToken cancellationToken);
    Task<Book> GetPrimaryBook(IUser currentUser, CancellationToken cancellationToken);
    Task<IList<Book>> GetAll(IUser currentUser, CancellationToken cancellationToken);
    Task<Book> Create(BookDto dto, IUser currentUser, CancellationToken cancellationToken);
    Task<Book> Update(long entityId, BookDto updateDto, IUser currentUser, CancellationToken cancellationToken);
    Task Delete(long bookId, IUser currentUser, CancellationToken cancellationToken);


}