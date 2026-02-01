using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.Import;
using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Extensions;
using DomuWave.Services.Implementations;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Dto.Transaction;
using DomuWave.Services.Models.Import;
using CPQ.Core.Consumers;
using CPQ.Core.DTO;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using Microsoft.Identity.Client;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class FinalizeImportCommandConsumer : InMemoryConsumerBase<FinalizeImportCommand, ImportDto>
{
    private IUserService _userService;
    private IImportService _importService;
    private IMediator _mediator;
    private IStoreCommitAction _storeCommitAction;
    public FinalizeImportCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService, IMediator mediator, IStoreCommitAction storeCommitAction) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
        _mediator = mediator;
        _storeCommitAction = storeCommitAction;
    }

    protected override async Task<ImportDto> Consume(FinalizeImportCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var importToUpdate = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
            .ConfigureAwait(false);
        if (importToUpdate == null)
            throw new NotFoundException("Import non trovato");


        var importedRows = await session.Query<ImportTransaction>().Where(k => k.Import.Id == evt.ImportId && k.ImportTransactionStatus == ImportTransactionStatus.Validated)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var importedStatus = await session.Query<TransactionStatus>().FirstOrDefaultAsync(k => k.Code == "imported", cancellationToken).ConfigureAwait(false);
        foreach (ImportTransaction importTransaction in importedRows)
        {

            //_storeCommitAction.EnqueueJob<ServiceJob>(j => j.FinalizeImportFromWeb(importTransaction.Id,currentUser.Id, CancellationToken.None));

            Models.Transaction transaction = new Models.Transaction();

            transaction.Category = importTransaction.Category;
            transaction.Beneficiary = importTransaction.Beneficiary;
            transaction.Account = importToUpdate.TargetAccount;
            transaction.Amount = Math.Abs(importTransaction.Amount.GetValueOrDefault());
            transaction.Currency = importTransaction.Currency;
            transaction.Description = importTransaction.Description;
            transaction.TransactionDate = importTransaction.TransactionDate;
            transaction.AccountCurrencyExchangeRate = importTransaction.AccountCurrencyExchangeRate.HasValue?importTransaction.AccountCurrencyExchangeRate.Value : 1;
            transaction.BookCurrencyExchangeRate = importTransaction.BookCurrencyExchangeRate.HasValue?importTransaction.BookCurrencyExchangeRate.Value : 1;
            transaction.FlowDirection = importTransaction.FlowDirection;
            transaction.TransactionType = importTransaction.TransactionType;
            transaction.PaymentMethod = importTransaction.PaymentMethod;
            transaction.Status = importedStatus;
            transaction.Book = importToUpdate.Book;
            transaction.Trace(currentUser);

            await session.SaveOrUpdateAsync(transaction, cancellationToken).ConfigureAwait(false);

            importTransaction.ImportTransactionStatus = ImportTransactionStatus.Imported;
            await session.SaveOrUpdateAsync(importTransaction, cancellationToken).ConfigureAwait(false);




        }

        long accountid = importToUpdate.TargetAccount.Id;
        long bookId = importToUpdate.TargetAccount.Book.Id;
        int currentUserId = currentUser.Id;
        _storeCommitAction.EnqueueJob<ServiceJob>(j => j.RecalculateAccountBalance(accountid, bookId, currentUserId, CancellationToken.None));

        return importToUpdate.ToDto();
    }
}