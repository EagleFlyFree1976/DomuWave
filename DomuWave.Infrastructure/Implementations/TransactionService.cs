using System.Runtime.Intrinsics.X86;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Category;
using DomuWave.Services.Command.Currency.ExchangeRate;
using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Consumers.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Helper;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Beneficiary;
using DomuWave.Services.Models.Dto.Currency;
using DomuWave.Services.Models.Dto.Transaction;
using Bogus.DataSets;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Hangfire;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using Remotion.Linq.Parsing;
using SimpleMediator.Core;
using Currency = DomuWave.Services.Models.Currency;

namespace DomuWave.Services.Implementations;

public class TransactionService : BaseService, ITransactionService
{
    private readonly IMediator _mediator;
    private readonly ITransactionStatusService _transactionStatusService;
    private readonly ITagService _tagService;
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ILogger<TransactionService> _log; // Add this line
    private readonly IUserService _userService;
    protected readonly IStoreCommitAction _storeCommitAction;
    public override string CacheRegion
    {
        get { return "Transaction"; }
    }


    public TransactionService(
        ISessionFactoryProvider sessionFactoryProvider,
        ICacheManager cache,
        IMediator mediator,
        ITransactionStatusService transactionStatusService,
        ITagService tagService,
        IBeneficiaryService beneficiaryService,
        ILogger<TransactionService> log, IStoreCommitAction storeCommitAction, IUserService userService) // Add ILogger to constructor
        : base(sessionFactoryProvider, cache)
    {
        _mediator = mediator;
        _transactionStatusService = transactionStatusService;
        _tagService = tagService;
        _beneficiaryService = beneficiaryService;
        _storeCommitAction = storeCommitAction;
        _userService = userService;
    }


    public IQueryable<Transaction> GetQueryable()
    {
        return session.Query<Transaction>().AsQueryable();
    }

    public async Task<Transaction> GetById(long itemId, long currentUserBookId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.GetAsync<Transaction>(itemId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(PaymentMethod paymentMethod, Category category)> ValidateTransaction(TransactionBaseDto item, Book book, IUser currentUser,
        CancellationToken cancellationToken)
    {
        PaymentMethod paymentMethod = null;

        if (item.Beneficiary == null)
        {
            throw new ValidatorException("Specificare il benificiario");
        }
        var account = await session.GetAsync<Account>(item.AccountId, cancellationToken).ConfigureAwait(false);

        if (account == null)
        {
            throw new ValidatorException("Specificare l'account");
        }

        if (account.Book.Id != book.Id)
        {
            throw new ValidatorException("L'account specificato non è valido");

        }

        if (!account.IsActive)
            throw new ValidatorException("L'account specificato non è attivo");



        GetPaymentMethodsForAccountCommand paymentMethodsForAccountCommand =
            new GetPaymentMethodsForAccountCommand(account.Id, currentUser.Id);
        var allPaymentMethodForAccount = await _mediator.GetResponse(paymentMethodsForAccountCommand, cancellationToken)
            .ConfigureAwait(false);

        (PaymentMethod paymentMethod, bool IsDefault) tuple = allPaymentMethodForAccount.Where(k => k.paymentMethod.Id == item.PaymentMethodId).FirstOrDefault();
        if (item.PaymentMethodId.HasValue)
        {

            if (tuple.paymentMethod == null)
            {
                throw new ValidatorException($"Il metodo di pagamento specificato non è valido");
            }
            else
            {
                paymentMethod = tuple.paymentMethod;
            }
        }
        else
        {
            paymentMethod = allPaymentMethodForAccount.FirstOrDefault(k => k.IsDefault).paymentMethod;


        }
        if (paymentMethod == null)
        {
            throw new ValidatorException($"Specificare il metodo di pagamento");
        }
        CanAccessToCategoryCommand canAccessToCategoryCommand =
            new CanAccessToCategoryCommand(item.CategoryId, currentUser.Id, book.Id);

        if (!await _mediator.GetResponse(canAccessToCategoryCommand).ConfigureAwait(false))
        {
            throw new ValidatorException("La categoria specificata non è valida");

        }

        var category = await session.GetAsync<Category>(item.CategoryId, cancellationToken).ConfigureAwait(false);

        if (category == null)
        {
            throw new ValidatorException("La categoria specificata non è valida");

        }

        if (item.TransactionType == TransactionType.Trasferimento)
        {
            if (!item.DestinationAccountId.HasValue)
            {
                throw new ValidatorException("Specificare l'account di destinazione per i trasferimenti");
            }
            var accountTo = await session.GetAsync<Account>(item.DestinationAccountId.Value, cancellationToken).ConfigureAwait(false);

            if (accountTo == null)
            {
                throw new ValidatorException("Specificare l'account di destinazione del trasferimento");
            }

            if (accountTo.Book.Id != book.Id)
            {
                throw new ValidatorException("L'account di destinazione specificato non è valido");

            }

            if (!accountTo.IsActive)
                throw new ValidatorException("L'account di destinazione specificato non è attivo");

        }



        return (paymentMethod, category);


    }

    public async Task<Transaction> Create(TransactionCreateDto item, long currentUserBookId, bool fromImport,
        IUser currentUser,
        CancellationToken cancellationToken)
    {
        Book? book =
            await session.GetAsync<Book>(currentUserBookId, cancellationToken).ConfigureAwait(false);

        CanAccessToBookCommand canAccessToBookCommand = new CanAccessToBookCommand(currentUser.Id, currentUserBookId);
        if (!await _mediator.GetResponse(canAccessToBookCommand, cancellationToken).ConfigureAwait(false))
        {
            throw new NotAllowedOperationException("Non hai accesso alla risorsa richiesta");
        }


        (PaymentMethod paymentMethod, Category category) info = await ValidateTransaction(item, book, currentUser, cancellationToken).ConfigureAwait(false);


        var account = await session.GetAsync<Account>(item.AccountId, cancellationToken).ConfigureAwait(false);

        var category = info.category;
        var paymenthMethod = info.paymentMethod;
        var currency = item.CurrencyId.HasValue ? await session.GetAsync<Currency>(item.CurrencyId, cancellationToken).ConfigureAwait(false) : account.Currency;

        if (item.TransactionType == TransactionType.Trasferimento)
        {
            var targetAccount = await session.GetAsync<Account>(item.DestinationAccountId.Value, cancellationToken).ConfigureAwait(false);
            if (targetAccount != null)
            {
                if (targetAccount.Id == account.Id)
                {
                    throw new ValidatorException("L'account di destinazione non può essere uguale a quello di origine");
                }

                currency = targetAccount.Currency;

            }

        }

        var beneficiary = item.Beneficiary != null && item.Beneficiary.Id != default(long) ?
                        await _beneficiaryService.GetById(item.Beneficiary.Id, currentUserBookId, currentUser, cancellationToken).ConfigureAwait(false) :
                await _beneficiaryService.GetByName(item.Beneficiary.Description, book.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (beneficiary == null)
        {
            beneficiary = await _beneficiaryService.Create(new BeneficiaryCreateUpdateDto()
            {
                BookId = book.Id,
                CategoryId = category.Id,
                Description = item.Beneficiary.Description
                    ,
                Iban = string.Empty,
                Name = item.Beneficiary.Description,
                Notes = string.Empty
            }, currentUserBookId, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }
        DateTime transactionDate = item.TransactionDate.HasValue ? item.TransactionDate.Value : DateTime.Now;
        var (bookCurrencyExchangeRate, accountCurrencyExchangeRate) = await GetBookAndAccountExchangeRate(transactionDate, currency, account, false, currentUser, cancellationToken);
        Guid transferKey = Guid.NewGuid();

        Transaction transaction = new Transaction
        {
            Book = book,
            Account = account,
            FlowDirection = item.TransactionType.ToFlowDirection(FlowDirection.Out),
            TransactionType = item.TransactionType,
            Amount = item.Amount,
            Currency = currency,
            BookCurrencyExchangeRate = bookCurrencyExchangeRate,
            AccountCurrencyExchangeRate = accountCurrencyExchangeRate,
            PaymentMethod = paymenthMethod,
            Category = category,
            Description = item.Description,
            TransactionDate = transactionDate,
        };
        transaction.Beneficiary = beneficiary;
        transaction.Trace(currentUser);
        transaction.Status = await _transactionStatusService.GetByIdAsync(item.Status, cancellationToken)
            .ConfigureAwait(false);

        if (item.TransactionType == TransactionType.Trasferimento)
        {
            transaction.TransferKey = transferKey;
        }



        await session.SaveAsync(transaction, cancellationToken).ConfigureAwait(false);
        if (item.Tags != null)
        {
            foreach (string itemTag in item.Tags)
            {
                Tag newTag = await this._tagService
                    .GetOrCreateTag(itemTag, currentUserBookId, currentUser, cancellationToken).ConfigureAwait(false);
                TransactionTag transactionTag = new TransactionTag
                {
                    Tag = newTag,
                    Transaction = transaction
                };
                transactionTag.Trace(currentUser);
                await session.SaveAsync(transactionTag, cancellationToken).ConfigureAwait(false);
            }
        }

        if (item.TransactionType == TransactionType.Trasferimento)
        {

            var otherTrans = await CreateOtherTransherTransaction(item, currentUserBookId, currentUser, cancellationToken, account, item.Amount, transactionDate, book, transaction, paymenthMethod, category, beneficiary, transferKey, item.Tags, fromImport);
            transaction.DestinationAccount = otherTrans.Account;
        }

        if (!fromImport)
        {

            _storeCommitAction.EnqueueJob<ServiceJob>(j => j.CalculateAccountBalance(transaction.Id, null,
                transaction.Account.Id,
                null, transaction.TransactionDate, book.Id, currentUser.Id, CancellationToken.None));
        }


        return transaction;
    }


    private async Task<Transaction> CreateOtherTransherTransaction(TransactionBaseDto item, long currentUserBookId,
        IUser currentUser,
        CancellationToken cancellationToken,
        Account sourceAccount,
        decimal sourceAmount,
        DateTime transactionDate, Book book,
        Transaction transaction, PaymentMethod paymenthMethod, Category category, Beneficiary beneficiary,
        Guid transferKey, IList<string> tags, bool fromImport)
    {
        decimal bookCurrencyExchangeRate;
        decimal accountCurrencyExchangeRate;
        Account otherAccount = await session.GetAsync<Account>(item.DestinationAccountId.Value, cancellationToken).ConfigureAwait(false);
        bookCurrencyExchangeRate = 1;
        accountCurrencyExchangeRate = 1;
        Currency currency = otherAccount.Currency;
        (bookCurrencyExchangeRate, accountCurrencyExchangeRate) = await GetBookAndAccountExchangeRate(transactionDate, currency, otherAccount, false, currentUser, cancellationToken);

        //ConvertToCurrencyCommand convertToCommand = new ConvertToCurrencyCommand(currentUser.Id, sourceAccount.Currency,
        //    otherAccount.Currency, sourceAmount, transactionDate);
        //ConvertResult? convertResult = await _mediator.GetResponse(convertToCommand, cancellationToken).ConfigureAwait(false);

        Transaction otherTransaction = new Transaction
        {
            Book = book,
            Account = otherAccount,
            FlowDirection = FlowDirection.In,
            TransactionType = TransactionType.Trasferimento,
            Amount = sourceAmount,
            Currency = currency,
            BookCurrencyExchangeRate = bookCurrencyExchangeRate,
            AccountCurrencyExchangeRate = accountCurrencyExchangeRate,
            PaymentMethod = paymenthMethod,
            Category = category,
            Description = item.Description,
            TransactionDate = transactionDate,
            Beneficiary = beneficiary,
            TransferKey = transferKey,
            DestinationAccount = sourceAccount
        };

        otherTransaction.Trace(currentUser);
        otherTransaction.Status = await _transactionStatusService.GetByIdAsync(item.Status, cancellationToken)
            .ConfigureAwait(false);

        otherTransaction.TransferKey = transferKey;




        await session.SaveAsync(otherTransaction, cancellationToken).ConfigureAwait(false);

        if (tags != null)
        {
            foreach (string itemTag in tags)
            {
                Tag newTag = await this._tagService
                    .GetOrCreateTag(itemTag, currentUserBookId, currentUser, cancellationToken).ConfigureAwait(false);
                TransactionTag transactionTag = new TransactionTag
                {
                    Tag = newTag,
                    Transaction = otherTransaction
                };
                transactionTag.Trace(currentUser);
                await session.SaveAsync(transactionTag, cancellationToken).ConfigureAwait(false);
            }
        }

        if (!fromImport)
        {
            BackgroundJob.Enqueue<ServiceJob>(j => j.CalculateAccountBalance(transaction.Id, null,
                otherTransaction.Account.Id,
                null, otherTransaction.TransactionDate, book.Id, currentUser.Id, CancellationToken.None));
        }

        return otherTransaction;
    }

    private async
        Task<(decimal bookCurrencyExchangeRate, decimal accountCurrencyExchangeRate)> GetBookAndAccountExchangeRate(
            DateTime transactionDate, Currency currency, Account account, bool exactlyMode, IUser currentUser,
            CancellationToken cancellationToken)
    {
        decimal bookCurrencyExchangeRate = 1;
        decimal accountCurrencyExchangeRate = 1;


        ///calcolo il tasso di cambio tra la valuta della transazione e quella del book
        if (currency.Id != account.Book.Currency.Id)
        {
            GetExchangeRateCommand findExchangeRateCommand = new GetExchangeRateCommand(currentUser.Id, transactionDate, currency.Id, account.Book.Currency.Id, exactlyMode);

            var exchangeRateInfo = await _mediator.GetResponse(findExchangeRateCommand, cancellationToken).ConfigureAwait(false);

            if (exchangeRateInfo == null)
            {
                throw new ValidatorException("Il tasso di cambio specificato non è valido");
            }
            bookCurrencyExchangeRate = exchangeRateInfo.Rate;
        }
        ///calcolo il tasso di cambio tra la valuta della transazione e quella dell'account
        if (currency.Id != account.Currency.Id)
        {
            GetExchangeRateCommand findExchangeRateCommand = new GetExchangeRateCommand(currentUser.Id, transactionDate, currency.Id, account.Currency.Id, exactlyMode);

            var exchangeRateInfo = await _mediator.GetResponse(findExchangeRateCommand, cancellationToken).ConfigureAwait(false);

            if (exchangeRateInfo == null)
            {
                throw new ValidatorException("Il tasso di cambio specificato non è valido");
            }
            accountCurrencyExchangeRate = exchangeRateInfo.Rate;
        }

        return (bookCurrencyExchangeRate, accountCurrencyExchangeRate);
    }



    public async Task<Transaction> Update(long entityId, TransactionUpdateDto updateDto, IUser currentUser,
        CancellationToken cancellationToken)
    {
        var transaction = await session.GetAsync<Transaction>(entityId, cancellationToken).ConfigureAwait(false);
        if (transaction == null)
        {
            throw new NotFoundException("Transazione non trovata");
        }

        List<Account> accountToRefresh = new List<Account>();
        accountToRefresh.Add(transaction.Account);
        if (transaction.DestinationAccount != null)
        {
            accountToRefresh.Add(transaction.DestinationAccount);
        }
        DateTime oldDate = transaction.TransactionDate;
        Account oldAccount = transaction.Account;
        Book book = transaction.Book;

        var oldTransactionType = transaction.TransactionType;
        var newTransactionType = updateDto.TransactionType;

        Transaction otherTransaction = null;

        CanAccessToBookCommand canAccessToBookCommand = new CanAccessToBookCommand(currentUser.Id, book.Id);
        if (!await _mediator.GetResponse(canAccessToBookCommand, cancellationToken).ConfigureAwait(false))
        {
            throw new NotAllowedOperationException("Non hai accesso alla risorsa richiesta");
        }

        await ValidateTransaction(updateDto, book, currentUser, cancellationToken).ConfigureAwait(false);


        var paymenthMethod = await session.GetAsync<PaymentMethod>(updateDto.PaymentMethodId, cancellationToken).ConfigureAwait(false);
        var account = await session.GetAsync<Account>(updateDto.AccountId, cancellationToken).ConfigureAwait(false);
        var category = await session.GetAsync<Category>(updateDto.CategoryId, cancellationToken).ConfigureAwait(false);
        var currency = updateDto.CurrencyId.HasValue ? await session.GetAsync<Currency>(updateDto.CurrencyId, cancellationToken).ConfigureAwait(false) : account.Currency;

        var beneficiary = updateDto.Beneficiary != null && updateDto.Beneficiary.Id != default(long) ?
                await _beneficiaryService.GetById(updateDto.Beneficiary.Id, book.Id, currentUser, cancellationToken).ConfigureAwait(false) :
                await _beneficiaryService.GetByName(updateDto.Beneficiary.Description, book.Id, currentUser, cancellationToken)
                    .ConfigureAwait(false);



        if (updateDto.TransactionType == TransactionType.Trasferimento)
        {
            var targetAccount = await session.GetAsync<Account>(updateDto.DestinationAccountId.Value, cancellationToken).ConfigureAwait(false);
            if (targetAccount != null)
            {
                if (targetAccount.Id == account.Id)
                {
                    throw new ValidatorException("L'account di destinazione non può essere uguale a quello di origine");
                }

                currency = targetAccount.Currency;

            }

        }



        if (beneficiary == null)
        {
            beneficiary = await _beneficiaryService.Create(new BeneficiaryCreateUpdateDto()
            {
                BookId = book.Id,
                CategoryId = category.Id,
                Description = updateDto.Beneficiary.Description,
                Iban = string.Empty,
                Name = updateDto.Beneficiary.Description,
                Notes = string.Empty
            }, book.Id, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }
        DateTime transactionDate = updateDto.TransactionDate.HasValue ? updateDto.TransactionDate.Value : DateTime.Now;

        bool isTransferRelatedTransaction = oldTransactionType == TransactionType.Trasferimento || updateDto.TransactionType == TransactionType.Trasferimento;


        var (bookCurrencyExchangeRate, accountCurrencyExchangeRate) = await GetBookAndAccountExchangeRate(transactionDate, currency, account, false, currentUser, cancellationToken);



        transaction.Account = account;
        transaction.Amount = updateDto.Amount;
        transaction.Currency = currency;

        transaction.BookCurrencyExchangeRate = bookCurrencyExchangeRate;
        transaction.AccountCurrencyExchangeRate = accountCurrencyExchangeRate;

        transaction.FlowDirection = updateDto.TransactionType.ToFlowDirection(FlowDirection.Out);
        transaction.TransactionType = updateDto.TransactionType;
        transaction.PaymentMethod = paymenthMethod;
        transaction.Category = category;
        transaction.Description = updateDto.Description;


        transaction.TransactionDate = transactionDate;
        transaction.Beneficiary = beneficiary;

        transaction.Status = await _transactionStatusService.GetByIdAsync(updateDto.Status, cancellationToken)
            .ConfigureAwait(false);

        transaction.Trace(currentUser);


        if (isTransferRelatedTransaction)
        {
            Guid? transferKey = transaction.TransferKey;
            if (oldTransactionType == TransactionType.Trasferimento && transferKey is not null)
            {
                otherTransaction = await session.Query<Transaction>().FirstOrDefaultAsync(k => k.TransferKey == transaction.TransferKey && k.Id != transaction.Id, cancellationToken).ConfigureAwait(false);
            }

            Account otherAccount = await session.GetAsync<Account>(updateDto.DestinationAccountId.GetValueOrDefault(), cancellationToken).ConfigureAwait(false);
            bookCurrencyExchangeRate = 1;
            accountCurrencyExchangeRate = 1;
            currency = otherAccount.Currency;
            (bookCurrencyExchangeRate, accountCurrencyExchangeRate) = await GetBookAndAccountExchangeRate(transactionDate, currency, otherAccount, false, currentUser, cancellationToken);


            if (newTransactionType == TransactionType.Trasferimento && (oldTransactionType != TransactionType.Trasferimento || !transferKey.HasValue))
            {
                // sto trasformando la transazione in un trasferimento, creo la chiave di trasferimento
                transferKey = Guid.NewGuid();

                var transactionTags = await session.Query<TransactionTag>().Where(k => k.Transaction.Id == transaction.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
                otherTransaction = await CreateOtherTransherTransaction(updateDto, transaction.Book.Id, currentUser, cancellationToken, account, updateDto.Amount, transactionDate, book, transaction, paymenthMethod, category, beneficiary, transferKey.Value,
                    transactionTags.Select(j => j.Tag.Name).ToList(), false);
                transaction.TransferKey = transferKey;
                transaction.DestinationAccount = otherAccount;
            }
            else
            {
                if (newTransactionType == TransactionType.Trasferimento &&
                    oldTransactionType == TransactionType.Trasferimento)
                {
                    // sto modificando una transazione di trasferimento, aggiorno la other transaction
                    //ConvertToCurrencyCommand convertToCommand = new ConvertToCurrencyCommand(currentUser.Id, account.Currency,
                    //    otherAccount.Currency, updateDto.Amount, transactionDate);
                    //ConvertResult? convertResult = await _mediator.GetResponse(convertToCommand, cancellationToken).ConfigureAwait(false);


                    otherTransaction.Account = otherAccount;
                    otherTransaction.Amount = updateDto.Amount;
                    otherTransaction.Currency = otherAccount.Currency;
                    otherTransaction.BookCurrencyExchangeRate = bookCurrencyExchangeRate;
                    otherTransaction.AccountCurrencyExchangeRate = accountCurrencyExchangeRate;
                    otherTransaction.FlowDirection = FlowDirection.In;
                    otherTransaction.TransactionType = TransactionType.Trasferimento;
                    otherTransaction.PaymentMethod = paymenthMethod;
                    otherTransaction.Category = category;
                    otherTransaction.Description = updateDto.Description;
                    otherTransaction.TransactionDate = transactionDate;
                    otherTransaction.Beneficiary = beneficiary;
                    otherTransaction.Status = transaction.Status;
                    otherTransaction.DestinationAccount = account;
                    otherTransaction.Trace(currentUser);
                    await session.SaveOrUpdateAsync(otherTransaction, cancellationToken).ConfigureAwait(false);


                }

                if (newTransactionType != TransactionType.Trasferimento &&
                    oldTransactionType == TransactionType.Trasferimento)
                {
                    // sto trasformando la transazione di trasferimento in una normale transazione, elimino la chiave di trasferimento
                    transferKey = null;
                    transaction.TransferKey = null;
                    transaction.DestinationAccount = null;
                    await session.SaveOrUpdateAsync(transaction, cancellationToken).ConfigureAwait(false);
                    // cancello la other transaction

                    await this.DeleteTagsForTransaction(otherTransaction.Id, currentUser, cancellationToken)
                        .ConfigureAwait(false);

                    await session.DeleteAsync(otherTransaction, cancellationToken).ConfigureAwait(false);
                }
            }




        }

        if (!accountToRefresh.Contains(transaction.Account))
        {
            accountToRefresh.Add(transaction.Account);
        }
        if (transaction.DestinationAccount != null && !accountToRefresh.Contains(transaction.DestinationAccount))
        {
            accountToRefresh.Add(transaction.DestinationAccount);
        }

        var distinct = accountToRefresh.Select(k => k.Id).Distinct().ToList();
        foreach (int toRefresh in distinct)
        {
            _storeCommitAction.EnqueueJob<ServiceJob>(j => j.CalculateAccountBalance(transaction.Id, toRefresh,
                    transaction.Account.Id,
                    oldDate, transaction.TransactionDate, book.Id, currentUser.Id, CancellationToken.None));

        }



        await session.SaveOrUpdateAsync(transaction, cancellationToken).ConfigureAwait(false);

        return transaction;
    }


    private async Task DeleteTagsForTransaction(long transactionId, IUser currentUser, CancellationToken cancellationToken)
    {
        var allTransactionTags = await session.Query<TransactionTag>().Where(k => k.Transaction.Id == transactionId).ToListAsync(cancellationToken).ConfigureAwait(false);


        foreach (TransactionTag transactionTag in allTransactionTags)
        {
            await session.DeleteAsync(transactionTag, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task Delete(long transactionId, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        await DeleteTagsForTransaction(transactionId, currentUser, cancellationToken).ConfigureAwait(false);
        var transaction = await session.GetAsync<Transaction>(transactionId, cancellationToken).ConfigureAwait(false);
        await session.DeleteAsync(transaction, cancellationToken).ConfigureAwait(false);
    }


    public async Task<PagedResult<Transaction>> Find(FindTransaction filters, int? pageNumber, int? pageSize, string sortBy, bool asc, IUser currentUser,
        CancellationToken cancellationToken)
    {
        CanAccessToBookCommand canAccessToBookCommand = new CanAccessToBookCommand(currentUser.Id, filters.BookId);
        if (!await _mediator.GetResponse(canAccessToBookCommand, cancellationToken).ConfigureAwait(false))
        {
            throw new NotAllowedOperationException("Non hai accesso alla risorsa richiesta");
        }
        var query = session.Query<Transaction>().FilterByBook<Transaction>(filters.BookId);
        query = query.Where(k => !k.Account.IsDeleted);


        if (filters.TargetAccountId.HasValue)
        {
            query = query.Where(k => (k.Account.Id == filters.TargetAccountId.Value && !k.Account.IsDeleted && k.Account.Book.Id == filters.BookId)
            || (k.DestinationAccount != null && k.DestinationAccount.Id == filters.TargetAccountId.Value && !k.DestinationAccount.IsDeleted && k.DestinationAccount.Book.Id == filters.BookId)
            );

            if (filters.TransactionType.HasValue)
            {
                if (filters.TransactionType == TransactionType.Trasferimento && filters.AccountId.HasValue)
                {
                    // cerco i trasferimenti tra i due account
                    query = query.Where(k => 
                                ((k.Account.Id == filters.TargetAccountId.Value && !k.Account.IsDeleted && k.Account.Book.Id == filters.BookId)
                                             && (k.DestinationAccount != null && k.DestinationAccount.Id == filters.AccountId.Value && !k.DestinationAccount.IsDeleted && k.DestinationAccount.Book.Id == filters.BookId))
                                             ||
                                ((k.Account.Id == filters.AccountId.Value && !k.Account.IsDeleted && k.Account.Book.Id == filters.BookId)
                                              && (k.DestinationAccount != null && k.DestinationAccount.Id == filters.TargetAccountId.Value && !k.DestinationAccount.IsDeleted && k.DestinationAccount.Book.Id == filters.BookId))
                    );
                }
                
                
            }
        }
            
        if (filters.AccountId.HasValue)
        {
            query = query.Where(k => k.Account.Id == filters.AccountId.Value && !k.Account.IsDeleted && k.Account.Book.Id == filters.BookId);
        }

        if (filters.Date != null && !filters.Date.IsEmpty())
        {
            if (filters.Date.StartDate.HasValue)
            {
                query = query.Where(k => k.TransactionDate >= filters.Date.StartDate.Value);
            }
            if (filters.Date.EndDate.HasValue)
            {
                var endDate = filters.Date.EndDate.Value.EndOfDay();
                query = query.Where(k => k.TransactionDate <= endDate);
            }
        }

        if (filters.CategoryId.HasValue)
        {
            query = query.Where(k => k.Category.Id == filters.CategoryId.Value && !k.Category.IsDeleted && k.Category.Book.Id == filters.BookId);
        }
        if (filters.Status.HasValue)
        {
            query = query.Where(k => k.Status.Id == filters.Status.Value);
        }

        if (filters.FlowDirection.HasValue)
        {
            query = query.Where(k => k.FlowDirectionCode == FlowDirectionMap.GetCode(filters.FlowDirection.Value));
        }

        if (filters.TransactionType.HasValue)
        {
            query = query.Where(k => k.TransactionTypeCode == TransactionTypeMap.GetCode(filters.TransactionType.Value));

        }

        if (!string.IsNullOrEmpty(filters.Note))
        {
            query = query.Where(k => k.Description.Contains(filters.Note));
        }

        if (string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDescending(k => k.TransactionDate);
        }
        else
        {
            switch (sortBy)
            {
                case "TransactionType":
                    query = asc ? query.OrderBy(k => k.TransactionTypeCode)
                        : query.OrderByDescending(k => k.TransactionTypeCode);
                    break;
                case "TransactionDate":
                    query = asc ? query.OrderBy(k => k.TransactionDate)
                        : query.OrderByDescending(k => k.TransactionDate);
                    break;
                case "Account":
                    query = asc ? query.OrderBy(k => k.Account.Description)
                        : query.OrderByDescending(k => k.Account.Description);
                    break;

                case "Status":
                    query = asc ? query.OrderBy(k => k.Status)
                        : query.OrderByDescending(k => k.Status);
                    break;

                case "Beneficiary":
                    query = asc ? query.OrderBy(k => k.Beneficiary)
                        : query.OrderByDescending(k => k.Beneficiary);
                    break;
                case "Category":
                    query = asc ? query.OrderBy(k => k.Category.Description)
                        : query.OrderByDescending(k => k.Category.Description);
                    break;

                case "Amount":
                    query = asc ? query.OrderBy(k => k.Amount)
                        : query.OrderByDescending(k => k.Amount);
                    break;
                case "Note":
                    query = asc ? query.OrderBy(k => k.Description)
                        : query.OrderByDescending(k => k.Description);
                    break;

                default:
                    query = query.OrderByDescending(k => k.TransactionDate); // fallback sicuro
                    break;
            }
        }

        // Calcolo totale record prima della paginazione
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Default values se non passati
        var currentPage = pageNumber.GetValueOrDefault(1);
        var currentSize = pageSize.GetValueOrDefault(20);

        // Applico la paginazione
        var items = await query
            .Skip((currentPage - 1) * currentSize)
            .Take(currentSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Transaction>
        {
            PageNumber = currentPage,
            PageSize = currentSize,
            TotalCount = totalCount,
            Items = items
        };


    }
    private async Task<(List<Transaction> allInputTransaction, List<Transaction> allOutputTransaction, DateTime? startDate, DateTime? endDate) >
        GetAllInputAndOutputTransactionForAccount(long accountId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var allTransactionForAccountQuery = session.Query<Transaction>()
            .Where(k => k.Account.Id == accountId && !k.IsDeleted /*&& k.IsEnabled*/)
            
            ;

        allTransactionForAccountQuery = allTransactionForAccountQuery.Where(k=>
                (!startDate.HasValue || k.TransactionDate >= startDate.Value) &&
                (!endDate.HasValue || k.TransactionDate <= endDate.Value)
            );
        allTransactionForAccountQuery = allTransactionForAccountQuery.Where( k=> k.Status.IncludeInBalance );



            //.ThenBy(k => k.Id)
        var allTransactionForAccount = await allTransactionForAccountQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        List<Transaction> allInputTransaction = new List<Transaction>();
        List<Transaction> allOutputTransaction = new List<Transaction>();
        foreach (Transaction transaction in allTransactionForAccount)
        {
            switch (transaction.TransactionType)
            {
                case TransactionType.Entrata:
                    allInputTransaction.Add(transaction);
                    break;
                case TransactionType.Uscita:
                    allOutputTransaction.Add(transaction);
                    break;
                case TransactionType.Trasferimento:
                    if (transaction.FlowDirection == FlowDirection.In)
                    {
                        allInputTransaction.Add(transaction);
                    }
                    else
                    {
                        allOutputTransaction.Add(transaction);
                    }
                    break;
            }
        }
        return (allInputTransaction, allOutputTransaction, startDate, endDate);
    }

    private async Task<decimal> GetAccountBalanceAtDate(long accountId, DateTime targetDate,
        CancellationToken cancellationToken)
    {
        var account = await session.GetAsync<Account>(accountId, cancellationToken).ConfigureAwait(false);
        if (account == null)
        {
            throw new NotFoundException("Account non trovato");
        }

        var allTransactionForAccount = await session.Query<Transaction>()
            .Where(k => k.Account.Id == accountId && !k.IsDeleted /*&& k.IsEnabled*/)
            .   Where(k=>k.TransactionDate < targetDate)
            .OrderBy(k => k.TransactionDate)
            //.ThenBy(k => k.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (allTransactionForAccount.Count == 0)
        {
            return 0;
        }
        decimal currentBalance = account.InitialBalance;
        foreach (Transaction transaction in allTransactionForAccount)
        {
            decimal absAmount = Math.Abs(transaction.AmountInAccountCurrency);
            switch (transaction.TransactionType)
            {
                case TransactionType.Entrata:
                    currentBalance += absAmount;
                    break;
                case TransactionType.Uscita:
                    currentBalance -= absAmount;
                    break;
                case TransactionType.Trasferimento:
                    if (transaction.FlowDirection == FlowDirection.In)
                    {
                        currentBalance += absAmount;
                    }
                    else
                    {
                        currentBalance -= absAmount;
                    }
                    break;
            }
        }

        return currentBalance;

    }
    public async Task RecalculateAccountBalance(long accountId, CancellationToken cancellationToken)
    {
        var account = await session.GetAsync<Account>(accountId, cancellationToken).ConfigureAwait(false);
        if (account == null)
        {
            throw new NotFoundException("Account non trovato");
        }
        var allTransactionForAccount = await session.Query<Transaction>()
            .Where(k => k.Account.Id == accountId && !k.IsDeleted /*&& k.IsEnabled*/)
            .OrderBy(k => k.TransactionDate)
            //.ThenBy(k => k.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (allTransactionForAccount.Count == 0)
        {
            return;
        }

        Dictionary<int, Transaction> orderedTransactions = new Dictionary<int, Transaction>();

        int index = 0;
        foreach (Transaction transaction in allTransactionForAccount)
        {
            orderedTransactions.Add(index, transaction);
            index++;
        }

        decimal currentBalance = account.InitialBalance;
        for (var i = 0; i< index; i++)
        {
            var transaction = orderedTransactions[i];
            if (i > 0)
            {
                var prevTransaction = orderedTransactions[i - 1];
                currentBalance = prevTransaction.AccountBalance.GetValueOrDefault();

            }

            decimal absAmount = Math.Abs(transaction.AmountInAccountCurrency);
            switch (transaction.TransactionType)
            {
                case TransactionType.Entrata:
                    currentBalance += absAmount;
                    break;
                case TransactionType.Uscita:
                    currentBalance -= absAmount;
                    break;
                case TransactionType.Trasferimento:
                    if (transaction.FlowDirection == FlowDirection.In)
                    {
                        currentBalance += absAmount;
                    }
                    else
                    {
                        currentBalance -= absAmount;
                    }
                    break;
            }
            transaction.AccountBalance = currentBalance;
            await session.SaveOrUpdateAsync(transaction, cancellationToken).ConfigureAwait(false);
        }

        // devo calcolare il bilancio del conto alla data odierna
        var balanceAtDate =
            await GetAccountBalanceAtDate(accountId, DateTime.Now, cancellationToken).ConfigureAwait(false);
        account.Balance = balanceAtDate;

        var nextMonth = DateTime.Now.AddMonths(1);
        var firstDateOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);

        var balanceAtEOM = await GetAccountBalanceAtDate(accountId, firstDateOfNextMonth, cancellationToken)
            .ConfigureAwait(false);

        var systemUser = await _userService.GetByIdAsync(1, cancellationToken).ConfigureAwait(false);
        account.Trace(systemUser);
        await session.SaveOrUpdateAsync(account, cancellationToken).ConfigureAwait(false);


        var accountReports = await session.Query<AccountReport>()
            .Where(k => k.Account.Id == accountId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);


        
        await CreateOrUpdateAccountReport(AccountReport.ReportActualBalance, DateTime.Now, DateTime.Now,  accountReports, account, balanceAtDate, systemUser, cancellationToken);

        await CreateOrUpdateAccountReport(AccountReport.ReportEOMBalance, DateTime.Now, DateTime.Now, accountReports, account, balanceAtEOM, systemUser, cancellationToken);


        await GeneratePeriodReport(AccountReport.ReportCurrentDayAllInput, AccountReport.ReportCurrentDayAllOutput, account,
            accountReports, DateTime.Now.Date, DateTime.Now.EndOfDay(), systemUser, cancellationToken).ConfigureAwait(false);
        
        await GeneratePeriodReport(AccountReport.ReportLastDayAllInput, AccountReport.ReportLastDayAllOutput, account,
            accountReports, DateTime.Now.AddDays(-1).Date, DateTime.Now.AddDays(-1).EndOfDay(), systemUser, cancellationToken).ConfigureAwait(false);




        await GeneratePeriodReport(AccountReport.ReportCurrentDayYTDAllInput, AccountReport.ReportCurrentDayYTDAllOutput, account,
            accountReports, DateTime.Now.AddYears(-1).Date, DateTime.Now.AddYears(-1).EndOfDay(), systemUser, cancellationToken).ConfigureAwait(false);


        await GeneratePeriodReport(AccountReport.ReportCurrentWeekAllInput, AccountReport.ReportCurrentWeekAllOutput, account,
            accountReports, DateTime.Now.FirstDayOfWeek(), DateTime.Now.LastDayOfWeek(), systemUser, cancellationToken).ConfigureAwait(false);



        await GeneratePeriodReport(AccountReport.ReportCurrentWeekYTDAllInput, AccountReport.ReportCurrentWeekYTDAllOutput, account,
            accountReports, DateTime.Now.AddYears(-1).FirstDayOfWeek(), DateTime.Now.AddYears(-1).LastDayOfWeek(), systemUser, cancellationToken).ConfigureAwait(false);


        await GeneratePeriodReport(AccountReport.ReportLastWeekAllInput, AccountReport.ReportLastWeekAllOutput, account,
            accountReports, DateTime.Now.FirstDayOfWeek().AddDays(-7), DateTime.Now.FirstDayOfWeek().AddDays(-7).LastDayOfWeek(), systemUser, cancellationToken).ConfigureAwait(false);



        await GeneratePeriodReport(AccountReport.ReportLastWeekYTDAllInput, AccountReport.ReportLastWeekYTDAllOutput, account,
            accountReports, DateTime.Now.FirstDayOfWeek().AddDays(-7), DateTime.Now.FirstDayOfWeek().AddDays(-7).LastDayOfWeek(), systemUser, cancellationToken).ConfigureAwait(false);

        await GeneratePeriodReport(AccountReport.ReportCurrentMonthAllInput, AccountReport.ReportCurrentMonthAllOutput,account,
            accountReports,DateTime.Now.FirstDayOfMonth(), DateTime.Now.LastDayOfMonth(), systemUser, cancellationToken).ConfigureAwait(false);
        
        
        
        await GeneratePeriodReport(AccountReport.ReportCurrentMonthYTDAllInput, AccountReport.ReportCurrentMonthYTDAllOutput,account,
            accountReports,DateTime.Now.AddYears(-1).FirstDayOfMonth(), DateTime.Now.AddYears(-1).LastDayOfMonth(), systemUser, cancellationToken).ConfigureAwait(false);
        
        await GeneratePeriodReport(AccountReport.ReportLastMonthAllInput, AccountReport.ReportLastMonthAllOutput, account,
            accountReports,DateTime.Now.AddMonths(-1).FirstDayOfMonth(), DateTime.Now.AddMonths(-1).LastDayOfMonth(), systemUser, cancellationToken).ConfigureAwait(false);
        
        await GeneratePeriodReport(AccountReport.ReportLastMonthYTDAllInput, AccountReport.ReportLastMonthYTDAllOutput, account,
            accountReports,DateTime.Now.AddYears(-1).AddMonths(-1).FirstDayOfMonth(), DateTime.Now.AddYears(-1).AddMonths(-1).LastDayOfMonth(), systemUser, cancellationToken).ConfigureAwait(false);


        await GeneratePeriodReport(AccountReport.ReportCurrentQuarterAllInput, AccountReport.ReportCurrentQuarterAllOutput, account,
            accountReports, DateTime.Now.FirstDayOfQuarter(), DateTime.Now.LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);
        
        await GeneratePeriodReport(AccountReport.ReportLastQuarterAllInput, AccountReport.ReportLastQuarterAllOutput, account,
            accountReports, DateTime.Now.FirstDayOfQuarter().AddDays(-1).FirstDayOfQuarter(), DateTime.Now.FirstDayOfQuarter().AddDays(-1).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);
        
        await GeneratePeriodReport(AccountReport.ReportCurrentQuarterYTDAllInput, AccountReport.ReportCurrentQuarterYTDAllOutput, account,
            accountReports, DateTime.Now.AddYears(-1).FirstDayOfQuarter(), DateTime.Now.AddYears(-1).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);


        await GeneratePeriodReport(AccountReport.ReportQuarterQ1AllInput, AccountReport.ReportQuarterQ1AllOutput, account,
            accountReports, DateTime.Now.FirstDayOfYear().FirstDayOfQuarter(), DateTime.Now.FirstDayOfYear().LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);
        
        await GeneratePeriodReport(AccountReport.ReportQuarterQ2AllInput, AccountReport.ReportQuarterQ2AllOutput, account,
            accountReports, DateTime.Now.FirstDayOfYear().AddMonths(3).FirstDayOfQuarter(), DateTime.Now.FirstDayOfYear().AddMonths(3).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);
        
        await GeneratePeriodReport(AccountReport.ReportQuarterQ3AllInput, AccountReport.ReportQuarterQ3AllOutput, account,
            accountReports, DateTime.Now.FirstDayOfYear().AddMonths(6).FirstDayOfQuarter(), DateTime.Now.FirstDayOfYear().AddMonths(6).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);

        await GeneratePeriodReport(AccountReport.ReportQuarterQ4AllInput, AccountReport.ReportQuarterQ4AllOutput, account,
            accountReports, DateTime.Now.FirstDayOfYear().AddMonths(9).FirstDayOfQuarter(), DateTime.Now.FirstDayOfYear().AddMonths(9).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);

        await GeneratePeriodReport(AccountReport.ReportQuarterQ1YTDAllInput, AccountReport.ReportQuarterQ1YTDAllOutput, account,
            accountReports, DateTime.Now.AddYears(-1).FirstDayOfYear().FirstDayOfQuarter(), DateTime.Now.AddYears(-1).FirstDayOfYear().LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);

        await GeneratePeriodReport(AccountReport.ReportQuarterQ2YTDAllInput, AccountReport.ReportQuarterQ2YTDAllOutput, account,
            accountReports, DateTime.Now.AddYears(-1).FirstDayOfYear().AddMonths(3).FirstDayOfQuarter(), DateTime.Now.AddYears(-1).FirstDayOfYear().AddMonths(3).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);

        await GeneratePeriodReport(AccountReport.ReportQuarterQ3YTDAllInput, AccountReport.ReportQuarterQ3YTDAllOutput, account,
            accountReports, DateTime.Now.AddYears(-1).FirstDayOfYear().AddMonths(6).FirstDayOfQuarter(), DateTime.Now.AddYears(-1).FirstDayOfYear().AddMonths(6).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);

        await GeneratePeriodReport(AccountReport.ReportQuarterQ4YTDAllInput, AccountReport.ReportQuarterQ4YTDAllOutput, account,
            accountReports, DateTime.Now.AddYears(-1).FirstDayOfYear().AddMonths(9).FirstDayOfQuarter(), DateTime.Now.AddYears(-1).FirstDayOfYear().AddMonths(9).LastDayOfQuarter(), systemUser, cancellationToken).ConfigureAwait(false);

    }

    private async Task GeneratePeriodReport(string inputReportKey,string outputReportKey, Account account, 
        List<AccountReport> accountReports,
        DateTime startDate, DateTime endDate,
        User systemUser, CancellationToken cancellationToken )
    {
        // calcolo il report per il mese corrente
        var lastMonthData = await GetAllInputAndOutputTransactionForAccount(account.Id, startDate, endDate, cancellationToken).ConfigureAwait(false);
        decimal totalInputLastMonth = lastMonthData.allInputTransaction.Sum(k => k.AmountInAccountCurrency);
        decimal totalOutputLastMonth = lastMonthData.allOutputTransaction.Sum(k => k.AmountInAccountCurrency);
        await CreateOrUpdateAccountReport(inputReportKey, lastMonthData.startDate.GetValueOrDefault(), lastMonthData.endDate.GetValueOrDefault(), accountReports, account, totalInputLastMonth, systemUser, cancellationToken);
        await CreateOrUpdateAccountReport(outputReportKey, lastMonthData.startDate.GetValueOrDefault(), lastMonthData.endDate.GetValueOrDefault(), accountReports, account, totalOutputLastMonth, systemUser, cancellationToken);


          }

    private async Task CreateOrUpdateAccountReport(string reportKey, DateTime startDate, DateTime endDate, List<AccountReport> accountReports,
        Account account, decimal balanceAtDate, User systemUser, CancellationToken cancellationToken)
    {
        var currentReportEOM = accountReports.FirstOrDefault(j => j.ReportKey == reportKey);
        if (currentReportEOM == null)
        {
            currentReportEOM = new AccountReport
            {
                Account = account,
                ReportKey = reportKey,
                ReportValue = balanceAtDate,
                StartDate = startDate,
                EndDate = endDate
            };
        }
        else
        {
            currentReportEOM.ReportValue= balanceAtDate;
            currentReportEOM.StartDate= startDate;
            currentReportEOM.EndDate= endDate;
        }

        currentReportEOM.Trace(systemUser);
        await session.SaveOrUpdateAsync(currentReportEOM, cancellationToken).ConfigureAwait(false);
    }

    public async Task<decimal> CalculateAccountBalance(long transactionId, long? prevAccountId, long actualAccountId,
        DateTime? prevTransactionDate, DateTime currentTransactionDate, long bookId, IUser currentUser,
        CancellationToken cancellationToken)
    {

        CanAccessToBookCommand canAccessToBookCommand = new CanAccessToBookCommand(currentUser.Id, bookId);
        if (!await _mediator.GetResponse(canAccessToBookCommand, cancellationToken).ConfigureAwait(false))
        {
            throw new NotAllowedOperationException("Non hai accesso alla risorsa richiesta");
        }

        var transaction = await session.GetAsync<Transaction>(transactionId, cancellationToken).ConfigureAwait(false);
        if (transaction == null)
        {
            throw new NotFoundException("Transazione non trovata");
        }

        // recupero tutte le transazioni prima della data della transazione specificata
        DateTime startDate = currentTransactionDate.Date;
        if (prevTransactionDate.HasValue && prevTransactionDate.Value < currentTransactionDate)
        {
            startDate = prevTransactionDate.Value.Date;
        }
        //recupero il saldo iniziale dell'account alla data precedente allo startdate
        var currentTransactionAccountBalance = await FillAccountBalanceForAccount(transactionId, actualAccountId, startDate, cancellationToken).ConfigureAwait(false);

        if (prevAccountId.HasValue && prevAccountId.Value != transaction.Account.Id)
        {
            await FillAccountBalanceForAccount(transactionId, prevAccountId.Value, startDate, cancellationToken).ConfigureAwait(false);
        }
        return currentTransactionAccountBalance;
    }

    private async Task<decimal> FillAccountBalanceForAccount(long transactionId, long accountId,
        DateTime startDate, CancellationToken cancellationToken)
    {
        var query = session.Query<Transaction>().Where(l => l.Account.Id == accountId);
        query = query.Where(l => !l.IsDeleted /*&& l.IsEnabled*/);
        //query = query.Where(l => l.Status.IncludeInBalance);
        query = query.Where(l => l.TransactionDate < startDate);
        var allPrev = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (Transaction transaction in allPrev)
        {
            decimal c = transaction.AmountInAccountCurrency;
            string g = "";
        }


        var initialBalance = await query
            .SumAsync(k => (decimal?)k.AmountInAccountCurrency, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var futureTransactionQuery = session.Query<Transaction>().Where(l => l.Account.Id == accountId);
        futureTransactionQuery = futureTransactionQuery.Where(l => !l.IsDeleted /*&& l.IsEnabled*/);
        futureTransactionQuery = futureTransactionQuery.Where(l => l.Status.IncludeInBalance);
        futureTransactionQuery = futureTransactionQuery.Where(l => l.TransactionDate >= startDate).OrderBy(k => k.TransactionDate);

        var futureTransaction = await futureTransactionQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

        decimal currentBalance = initialBalance;
        decimal currentTransactionAccountBalance = 0;
        foreach (Transaction currentTran in futureTransaction)
        {
            if (currentTran.Status.IncludeInBalance)
            {
                switch (currentTran.TransactionType)
                {
                    case TransactionType.Entrata:
                        currentBalance += currentTran.AmountInAccountCurrency;
                        break;
                    case TransactionType.Uscita:
                        currentBalance -= currentTran.AmountInAccountCurrency;
                        break;
                    case TransactionType.Trasferimento:
                        if (currentTran.FlowDirection == FlowDirection.In)
                        {
                            currentBalance += currentTran.AmountInAccountCurrency;
                        }
                        else
                        {
                            currentBalance -= currentTran.AmountInAccountCurrency;
                        }
                        break;
                }

                if (currentTran.Id == transactionId)
                {
                    currentTransactionAccountBalance = currentBalance;
                }
            }
            currentTran.AccountBalance = currentBalance;
            await session.SaveOrUpdateAsync(currentTran, cancellationToken).ConfigureAwait(false);
        }

        return currentTransactionAccountBalance;
    }
}