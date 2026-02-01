using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Exceptions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using DomuWave.Services.Extensions;
using CPQ.Core.Extensions;
using NHibernate.Proxy;

namespace DomuWave.Services.Implementations;

public class BookService : BaseService, IBookService
{
    public override string CacheRegion
    {
        get { return "Book"; }
    }
    public BookService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
    {
    }

    public async Task<Book> GetSystem(IUser currentUser, CancellationToken cancellationToken)
    {
        Book targetBook = await session.Query<Book>().GetQueryable().Where(k => k.IsSystem)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return targetBook;
    }
    public async Task<Book> GetById(long itemId,  CancellationToken cancellationToken)
    {
        Book targetBook = await session.GetAsync<Book>(itemId, cancellationToken).ConfigureAwait(false);

        return targetBook;

    }
    public async Task<Book> GetById(long itemId, IUser currentUser, CancellationToken cancellationToken)
    {
        Book targetBook = await session.Query<Book>().GetQueryable().FilterByOwner(currentUser).Where(k => k.Id == itemId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return targetBook;

    }

    public async Task<Book> GetPrimaryBook(IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<Book>().Where(k => !k.IsDeleted && k.OwnerId == currentUser.Id && k.IsPrimary)
            .FirstOrDefaultAsync(cancellationToken);

    }

    public async Task<IList<Book>> GetAll(IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<Book>().Where(k => !k.IsDeleted && k.OwnerId == currentUser.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book> Create(BookDto dto, IUser currentUser, CancellationToken cancellationToken)
    {
        if (dto.IsPrimary)
        {
            // verifico che l'utenrte non abbia già un book primario

            var existsPrimary = await this.GetPrimaryBook(currentUser, cancellationToken).ConfigureAwait(false);
            if (existsPrimary != null)
                throw new ValidatorException($"Esiste già il book principale");
        }

        if (string.IsNullOrEmpty(dto.Name))
        {
            throw new ValidatorException("Specificare il nome");
        }

        IQueryable<Book> queryable = session.Query<Book>();

        var existsByName = await queryable.GetQueryable().FilterByOwner(currentUser)
            .Where(k => k.Name == dto.Name).AnyAsync(cancellationToken);
        if (existsByName)
        {
            throw new ValidatorException($"Esiste già un book con questo nome");
        }

        Currency currency = null;
        if (dto.CurrencyId.HasValue)
        {
            currency = await session.GetAsync<Currency>(dto.CurrencyId.Value, cancellationToken).ConfigureAwait(false);
            if (currency == null)
                throw new NotFoundException("Valuta non trovata");

            if (currency.IsDeleted)
                throw new ValidatorException("Valuta non valida");

        }
        else
        {
            currency = await session.Query<Currency>().Where(k => k.IsDefault && !k.IsDeleted).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        Book newBook = new Book()
            { Name = dto.Name, Description = dto.Description, OwnerId = dto.OwnerId, IsPrimary = dto.IsPrimary, Currency = currency};
        
        newBook.Trace(currentUser);

        await session.SaveAsync(newBook, cancellationToken).ConfigureAwait(false);

        return newBook;

    }

    public async Task<Book> Update(long entityId, BookDto updateDto, IUser currentUser, CancellationToken cancellationToken)
    {
        var book = await session.GetAsync<Book>(entityId, cancellationToken).ConfigureAwait(false);

        if (book != null)
        {
            book.IsPrimary = updateDto.IsPrimary;
            book.Name = updateDto.Name;
            book.Description = updateDto.Description;
            book.OwnerId = updateDto.OwnerId;
            await session.SaveOrUpdateAsync(book, cancellationToken).ConfigureAwait(false);
        }

        return book;
    }

    public async Task Delete(long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        Book bookToDelete = await session.GetAsync<Book>(bookId, cancellationToken).ConfigureAwait(false);

        if (bookToDelete == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }

        bookToDelete.IsDeleted = true;
        await session.SaveOrUpdateAsync(bookToDelete, cancellationToken).ConfigureAwait(false);

        var allAccountToDelete =
            await session.Query<Account>().FilterByBook(bookToDelete).ToListAsync(cancellationToken);

        //todo

    }
}