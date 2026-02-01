using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using NHibernate.Linq;
using NHibernate.Proxy;
using Refit;

namespace DomuWave.Services.Implementations;

public class AccountService : BaseService, IAccountService
{
    public override string CacheRegion
    {
        get { return "Account"; }
    }

    public AccountService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, IStoreCommitAction storeCommitAction) : base(sessionFactoryProvider, cache)
    {
        _storeCommitAction = storeCommitAction;
    }

    public async Task<Account> GetById(long itemId, IUser currentUser, CancellationToken cancellationToken)
    {
        Account targetAccount = await session.Query<Account>().GetQueryable().FilterByOwner(currentUser).Where(k => k.Id == itemId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return targetAccount;

    }

    

    public async Task<IList<Account>> GetAll(IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<Account>().Where(k => !k.IsDeleted && k.OwnerId == currentUser.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task<(AccountType type, Book book, Currency currency)> Validate(Account accountToUpdate, 
        string name, int accountTypeId, DateTime? closedDate, long? bookId, int currencyId, IUser currentUser,
        CancellationToken cancellationToken)
    {
        
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidatorException("Specificare il nome");
        }

        IQueryable<Account> queryable = session.Query<Account>();

        IQueryable<Account> accounts = queryable.GetQueryable().FilterByOwner(currentUser)
            .Where(k => k.Name == name);

        if (accountToUpdate != null)
        {
            accounts = accounts.Where(j => j.Id != accountToUpdate.Id);
        }
        var existsByName = await accounts.AnyAsync(cancellationToken);
        if (existsByName)
        {
            throw new ValidatorException($"Esiste già un Accounts con questo nome");
        }
        AccountType accountType = await session.GetAsync<AccountType>(accountTypeId, cancellationToken);
        Book book = await session.Query<Book>().Where(k => k.Id == bookId)
            .FirstOrDefaultAsync(cancellationToken);

        if (accountToUpdate == null)
        {
            if (accountType == null)
            {
                throw new ValidatorException("Specificare la tipologia di account");
            }
            if (book == null)
            {
                throw new ValidatorException("Specificare il book di appartenenza");
            }
        }
        else
        {
            if (closedDate.HasValue)
            {
                if (closedDate.Value < accountToUpdate.OpenDate)
                {
                    throw new ValidatorException(
                        "La data di chiusura del conto non è valida, deve essere successiva alla data di apertura");
                }
            }
        }
            Currency currency = await session.GetAsync<Currency>(currencyId, cancellationToken).ConfigureAwait(false);

        if (currency == null)
        {
            throw new ValidatorException($"Specificare la valuta");
        }

        return (accountType, book, currency);
    }
    public async Task<Account> Create(AccountCreateDto createDto, IUser currentUser, CancellationToken cancellationToken)
    {

      var data =   await Validate(null, createDto.Name, createDto.AccountTypeId, null,createDto.BookId, createDto.CurrencyId,
            currentUser,cancellationToken).ConfigureAwait(false);

        

        Account newAccount = new Account()
        {
            Name = createDto.Name, Description = createDto.Description, OwnerId = createDto.OwnerId,AccountType = data.type, InitialBalance = createDto.InitialBalance.GetValueOrDefault(),
            Book = data.book, OpenDate = createDto.OpenDate, ClosedDate = createDto.ClosedDate, IsActive = true, Currency = data.currency
            

        };
        
        newAccount.Trace(currentUser);

        await session.SaveAsync(newAccount, cancellationToken).ConfigureAwait(false);

        return newAccount;

    }
    protected readonly IStoreCommitAction _storeCommitAction;
    public async Task<Account> Update(long entityId, AccountUpdateDto updateDto, IUser currentUser, CancellationToken cancellationToken)
    {
        var account = await session.GetAsync<Account>(entityId, cancellationToken).ConfigureAwait(false);
        if (account != null)
        {
            var prevInitialBalance = account.InitialBalance;
            var newInitialBalance = updateDto.InitialBalance;
            if (newInitialBalance != prevInitialBalance)
            {
                _storeCommitAction.EnqueueJob<ServiceJob>(j => j.RecalculateAccountBalance(account.Id, account.Book.Id, currentUser.Id, CancellationToken.None));
            }

            var data = await Validate(account, updateDto.Name, account.AccountType.Id,updateDto.ClosedDate, account.Book.Id, updateDto.CurrencyId,
                currentUser, cancellationToken).ConfigureAwait(false);

            account.Name = updateDto.Name;
            account.Description = updateDto.Description;
            account.InitialBalance = updateDto.InitialBalance.GetValueOrDefault();
            account.OpenDate = updateDto.OpenDate;
            account.ClosedDate= updateDto.ClosedDate;


            account.Currency = await session.GetAsync<Currency>(updateDto.CurrencyId, cancellationToken)
                .ConfigureAwait(false);
            account.Trace(currentUser);
            await session.SaveOrUpdateAsync(account, cancellationToken).ConfigureAwait(false);
            return account;
        }

        throw new NotFoundException($"Account non trovato, verificare i dati");


    }

    public async Task Delete(long accountId, IUser currentUser, CancellationToken cancellationToken)
    {
        var account = await session.GetAsync<Account>(accountId, cancellationToken).ConfigureAwait(false);

        account.IsDeleted = true;
        account.Trace(currentUser);


        // cancello tutte le transazioni 
        var transactions = await session.Query<Transaction>().Where(j => j.Account.Id == accountId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var transaction in transactions)
        {
            transaction.IsDeleted = true;
            transaction.Trace(currentUser);
            await session.SaveOrUpdateAsync(transaction, cancellationToken).ConfigureAwait(false);
        }
        await session.SaveOrUpdateAsync(account, cancellationToken).ConfigureAwait(false);
    }
}