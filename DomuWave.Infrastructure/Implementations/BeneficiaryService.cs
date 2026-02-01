using DomuWave.Services.Command.Beneficiary;
using DomuWave.Services.Command.PaymentMethod;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Beneficiary;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Implementations;

public class BeneficiaryService : BaseService, IBeneficiaryService
{
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;

    public override string CacheRegion
    {
        get { return "Beneficiary"; }
    }

    public BeneficiaryService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider, cache)
    {
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    public async Task<Beneficiary> GetById(long itemId, long currentUserBookId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.GetAsync<Beneficiary>(itemId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<Beneficiary>> GetAll(IUser currentUser, long? bookId, CancellationToken cancellationToken)
    {
        return await session.Query<Beneficiary>().Where( i=>i.Book.Id == bookId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    private async Task Validate(long? itemId, Book book, string name, IUser currentUser,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ValidatorException("Specificare il nome");
        }

        
        CanAccessToBeneficiaryCommand canAccessToBeneficiaryCommand =
            new CanAccessToBeneficiaryCommand(itemId, currentUser.Id, book.Id);
        if (!await _mediator.GetResponse(canAccessToBeneficiaryCommand).ConfigureAwait(false))
        {
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }

        IQueryable<Beneficiary> queryable = session.Query<Beneficiary>();

        queryable = queryable.GetQueryable<Beneficiary, long>()
            .FilterByOwner<Beneficiary, long>(book, new bool?(), currentUser)

            .Where(k => k.Name == name);

        if (itemId.HasValue)
        {
            queryable = queryable.Where(j => j.Id != itemId);
        }

        var existsByName = await queryable.AnyAsync(cancellationToken);
        if (existsByName)
        {
            throw new ValidatorException($"Esiste già un elemento con questo nome, [${name}]");
        }



    }
    public async Task<Beneficiary> Create(BeneficiaryCreateUpdateDto dto, long currentUserBookId, IUser currentUser,
        CancellationToken cancellationToken)
    {
        Book? book =
            await session.GetAsync<Book>(dto.BookId, cancellationToken).ConfigureAwait(false);


        await Validate(null, book, dto.Name, currentUser, cancellationToken).ConfigureAwait(false);

        Category _category = null;

        if (dto.CategoryId.HasValue)
        {
            _category = await session.GetAsync<Category>(dto.CategoryId.Value, cancellationToken).ConfigureAwait(false);
        }
        
        Beneficiary item = new Beneficiary();
        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Iban = dto.Iban;
        item.Notes = dto.Notes;
        item.Book = book;
        item.Category = _category;
        item.IsEnabled = true;
        item.Trace(currentUser);
        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);

        return item;
    }

    public async Task<Beneficiary> Update(long entityId, BeneficiaryCreateUpdateDto dto, IUser currentUser,
        CancellationToken cancellationToken)
    {
        Book? book = await session.GetAsync<Book>(dto.BookId, cancellationToken).ConfigureAwait(false);


        await Validate(entityId, book, dto.Name, currentUser, cancellationToken).ConfigureAwait(false);

        Beneficiary item = await session.GetAsync<Beneficiary>(entityId, cancellationToken).ConfigureAwait(false);

        Category _category = null;

        if (dto.CategoryId.HasValue)
        {
            _category = await session.GetAsync<Category>(dto.CategoryId.Value, cancellationToken).ConfigureAwait(false);
        }
        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Iban = dto.Iban;
        item.Notes = dto.Notes;
        item.Book = book;
        item.Category = _category;
        item.Trace(currentUser);
        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task Delete(long beneficiaryId, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        var existsTransactions = await session.Query<Transaction>()
            .Where(k => !k.IsDeleted && k.Beneficiary.Id == beneficiaryId).AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existsTransactions)
        {
            throw new ValidatorException(
                "Non è possibile cancellare questo beneficiario in quanto esistono transazioni che lo utilizzano");

        }

        var beneficiary = await session.GetAsync<Beneficiary>(beneficiaryId, cancellationToken).ConfigureAwait(false);
        if (beneficiary == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }

         
        beneficiary.IsDeleted = true;
        beneficiary.Trace(currentUser);

        await session.SaveOrUpdateAsync(beneficiary, cancellationToken).ConfigureAwait(false);
    }


    public async  Task<Beneficiary> GetByName(string name, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        var findBook = await session.Query<Beneficiary>().FilterByBook(bookId).Where(j => j.Name == name)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return findBook;
    }

    public async Task<IList<Beneficiary>> Find(string q, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {

        var query = session.Query<Beneficiary>().FilterByBook(bookId);
        if (!string.IsNullOrEmpty(q))
        {
            query = query.Where(j => j.Name.Contains(q) || j.Description.Contains(q));
        }
        var beneficiaries = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return beneficiaries;
    }


    public Task ClearCache(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}