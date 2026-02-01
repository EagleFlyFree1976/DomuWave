using System.Security.Principal;
using System.Threading;
using DomuWave.Services.Command.Category;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Category;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Implementations;

public class CategoryService : BaseService, ICategoryService
{
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
        
    public override string CacheRegion
    {
        get { return "Category"; }
    }
   
    public CategoryService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider, cache)
    {
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }


    public async  Task<Category> GetById(long CategoryId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.GetAsync<Category>(CategoryId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Category> GetById(long itemId, long bookId, IUser currentUser,
        CancellationToken cancellationToken)
    {
        return await session.GetAsync<Category>(itemId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<Category>> GetAll(IUser currentUser, long? bookId, CancellationToken cancellationToken)
    {
        return await session.Query<Category>().GetQueryable<Category, long>().FilterByBook<Category, long>(bookId).ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    }

    private async Task Validate(long? itemId, Book book, string name, IUser currentUser,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ValidatorException("Specificare il nome");
        }

        CanAccessToCategoryCommand canAccessToCategoryCommand =
            new CanAccessToCategoryCommand(itemId, currentUser.Id, book.Id);
        if (!await _mediator.GetResponse(canAccessToCategoryCommand).ConfigureAwait(false))
        {
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }

        IQueryable<Category> queryable = session.Query<Category>();

        queryable = queryable.GetQueryable<Category, long>()
            .FilterByOwner<Category, long>(book, new bool?(), currentUser)

            .Where(k => k.Name == name);

        if (itemId.HasValue)
        {
            queryable = queryable.Where(j => j.Id != itemId);
        }

        var existsByName = await queryable.AnyAsync(cancellationToken);
        if (existsByName)
        {
            throw new ValidatorException($"Esiste già un elemento con questo nome");
        }



    }

    public async Task<Category> Create(CategoryCreateUpdateDto dto, long currentUserBookId, IUser currentUser,
        CancellationToken cancellationToken)
    {

        Book? book = 
             await session.GetAsync<Book>(dto.BookId, cancellationToken).ConfigureAwait(false);
            

        await Validate(null, book, dto.Name, currentUser, cancellationToken).ConfigureAwait(false);


        Category item = new Category();
        Category parentCategory = null;

        if (dto.ParentCategoryId.HasValue)
        {
            parentCategory = await session.GetAsync<Category>(dto.ParentCategoryId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (parentCategory == null || parentCategory.Book.Id != book.Id)
            {
                throw new NotFoundException($"Non è stato possibile trovare la categoria padre");
            }
        }
        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Book = book;
        item.IsEnabled = true;
        item.Trace(currentUser);
        item.ParentCategory = parentCategory;
        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);

        return item;
    }

    public async Task<Category> Update(long entityId, CategoryCreateUpdateDto dto,
        IUser currentUser,
        CancellationToken cancellationToken)
    {
        Book? book = await session.GetAsync<Book>(dto.BookId, cancellationToken).ConfigureAwait(false);
            

        await Validate(entityId, book, dto.Name, currentUser, cancellationToken).ConfigureAwait(false);

        Category item = await session.GetAsync<Category>(entityId, cancellationToken).ConfigureAwait(false);
        Category parentCategory = null;

        if (dto.ParentCategoryId.HasValue)
        {
            parentCategory = await session.GetAsync<Category>(dto.ParentCategoryId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (parentCategory == null || parentCategory.Book.Id != book.Id)
            {
                throw new NotFoundException($"Non è stato possibile trovare la categoria padre");
            }
        }
        item.Name = dto.Name;
        item.Description = dto.Description;
        item.ParentCategory = parentCategory;
        item.Trace(currentUser);

        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);
        return item;
    }


    
    public async Task Delete(long categoryId, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        //esistono transazioni associate a questa categoria?

        var existsTransactions = await session.Query<Transaction>()
            .Where(k => !k.IsDeleted && k.Category.Id == categoryId).AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existsTransactions)
        {
            throw new ValidatorException(
                "Non è possibile cancellare questa categoria in quanto esistono transazioni che la utilizzano");

        }

        // esistono delle sotto categorie?

        var existsSubCategory = await session.Query<Category>().FilterByBook(bookId)
            .Where(j => !j.IsDeleted && j.ParentCategory != null && j.ParentCategory.Id == categoryId)
            .AnyAsync(cancellationToken).ConfigureAwait(false);
        if (existsSubCategory)
        {
            throw new ValidatorException(
                "Non è possibile cancellare questa categoria in quanto esistono delle categorie figlie che la utilizzano");

        }

        var category = await session.GetAsync<Category>(categoryId, cancellationToken).ConfigureAwait(false);
        if (category == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }

       
        category.IsDeleted = true;
        category.Trace(currentUser);

        await session.SaveOrUpdateAsync(category, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<Category>> Find(long bookId, string q, bool includeAll, IUser currentUser,
        CancellationToken cancellationToken)
    {
        IQueryable<Category> queryable = session.Query<Category>().GetQueryable<Category, long>();
        
            if (!includeAll)
            {
                queryable = queryable.Where(k => k.Book != null && k.Book.Id == bookId);
            }
            else
            {
                queryable = queryable.Where(k => k.Book.IsSystem || k.Book.Id == bookId);

            }

            if (!string.IsNullOrEmpty(q))
            {
                queryable = queryable.Where(k => k.Name.Contains(q) || k.Description.Contains(q));
            }

            var allCategories = await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);

            var allCategoryToAdd = allCategories
                .Where(k => k.ParentCategory != null && !allCategories.Any(j => j.Id == k.ParentCategory.Id))
                .Select(l => l.ParentCategory).Distinct().ToList();

            foreach (var l in allCategoryToAdd)
            {
                allCategories.Add(l);
            }


            return allCategories;
    }
}