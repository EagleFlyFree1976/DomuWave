using System.Security.Principal;
using System.Threading;
using DomuWave.Services.Command.PaymentMethod;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Implementations;

public class PaymentMethodService : BaseService, IPaymentMethodService
{
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;

    public PaymentMethodService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider, cache)
    {
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    public override string CacheRegion
    {
        get { return "PaymentMethod"; }
    }

    public Task ClearCache(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public async  Task<PaymentMethod> GetById(int paymentMethodId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.GetAsync<PaymentMethod>(paymentMethodId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PaymentMethod> GetById(int itemId, long bookId, IUser currentUser,
        CancellationToken cancellationToken)
    {
        return await session.GetAsync<PaymentMethod>(itemId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<PaymentMethod>> GetAll(IUser currentUser, long? bookId, CancellationToken cancellationToken)
    {
        return await session.Query<PaymentMethod>().GetQueryable<PaymentMethod, int>().FilterByBook<PaymentMethod, int>(bookId).ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    }

    private async Task Validate(int? itemId, Book book, string name, IUser currentUser,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ValidatorException("Specificare il nome");
        }

        CanAccessToPaymentMethodCommand canAccessToPaymentMethodCommand =
            new CanAccessToPaymentMethodCommand(itemId, currentUser.Id, book.Id);
        if (!await _mediator.GetResponse(canAccessToPaymentMethodCommand).ConfigureAwait(false))
        {
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }

        IQueryable<PaymentMethod> queryable = session.Query<PaymentMethod>();

        queryable = queryable.GetQueryable<PaymentMethod, int>()
            .FilterByOwner<PaymentMethod, int>(book, new bool?(), currentUser)

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

    public async Task<PaymentMethod> Create(PaymentMethodCreateUpdateDto dto, long currentUserBookId, IUser currentUser,
        CancellationToken cancellationToken)
    {

        Book? book = 
             await session.GetAsync<Book>(dto.BookId, cancellationToken).ConfigureAwait(false);
            

        await Validate(null, book, dto.Name, currentUser, cancellationToken).ConfigureAwait(false);


        PaymentMethod item = new PaymentMethod();

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Book = book;
        item.IsEnabled = true;
        item.Trace(currentUser);
        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);

        return item;
    }

    public async Task<PaymentMethod> Update(int entityId, PaymentMethodCreateUpdateDto dto,
        IUser currentUser,
        CancellationToken cancellationToken)
    {
        Book? book = await session.GetAsync<Book>(dto.BookId, cancellationToken).ConfigureAwait(false);
            

        await Validate(entityId, book, dto.Name, currentUser, cancellationToken).ConfigureAwait(false);

        PaymentMethod item = await session.GetAsync<PaymentMethod>(entityId, cancellationToken).ConfigureAwait(false);


        item.Name = dto.Name;
        item.Description = dto.Description;
 
        item.Trace(currentUser);
        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);

        return item;
    }


    public async  Task Disable(int paymentMethodId, IUser currentUser, CancellationToken cancellationToken)
    {
        PaymentMethod item = await session.GetAsync<PaymentMethod>(paymentMethodId, cancellationToken).ConfigureAwait(false);
        await Validate(paymentMethodId, item.Book, item.Name, currentUser, cancellationToken).ConfigureAwait(false);

        item.IsEnabled = false;

        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async  Task Enable(int paymentMethodId, IUser currentUser, CancellationToken cancellationToken)
    {
        PaymentMethod item = await session.GetAsync<PaymentMethod>(paymentMethodId, cancellationToken).ConfigureAwait(false);
        await Validate(paymentMethodId, item.Book, item.Name, currentUser, cancellationToken).ConfigureAwait(false);

        item.IsEnabled = true;

        await session.SaveAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async Task Delete(int categoryId, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        //esistono degli account con questo metodo di pagamento?

        var existsAccount = await session.Query<AccountPaymentMethod>()
            .Where(k => !k.IsDeleted && k.PaymentMethod.Id == categoryId).AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existsAccount)
        {
            throw new ValidatorException(
                "Non è possibile cancellare questo metodo di pagamento in quando esistono accounts che lo utilizzano");

        }

        var paymentMethod = await session.GetAsync<PaymentMethod>(categoryId, cancellationToken).ConfigureAwait(false);
        if (paymentMethod == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }

        var allPaymentAccountType = await session.Query<AccountTypePaymentMethod>()
            .Where(k => k.PaymentMethod.Id == categoryId).ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (AccountTypePaymentMethod method in allPaymentAccountType)
        {
            method.IsDeleted = true;
            method.Trace(currentUser);
            await session.SaveOrUpdateAsync(method, cancellationToken).ConfigureAwait(false);
        }

        paymentMethod.IsDeleted = true;
        paymentMethod.Trace(currentUser);

        await session.SaveOrUpdateAsync(paymentMethod, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<PaymentMethod>> Find(long bookId, bool includeAll, IUser currentUser,
        CancellationToken cancellationToken)
    {
        IQueryable<PaymentMethod> queryable = session.Query<PaymentMethod>().GetQueryable<PaymentMethod, int>();
        
            if (!includeAll)
            {
                queryable = queryable.Where(k => k.Book != null && k.Book.Id == bookId);
            }
            else
            {
                queryable = queryable.Where(k => k.Book.IsSystem || k.Book.Id == bookId);

            }
        

        return await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}