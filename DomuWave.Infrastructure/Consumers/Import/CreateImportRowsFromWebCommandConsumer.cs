using System.Collections.Generic;
using DomuWave.Services.Command.Import;
using DomuWave.Services.Helper;
using DomuWave.Services.Implementations;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Import;
using DomuWave.Services.Models.Import;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class CreateImportRowsFromWebCommandConsumer : InMemoryConsumerBase<CreateImportRowsFromWebCommand, bool>
{
    private IUserService _userService;
    private IImportService _importService;
    private IStoreCommitAction _storeCommitAction;
    public CreateImportRowsFromWebCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService, IStoreCommitAction storeCommitAction) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
        _storeCommitAction = storeCommitAction;
    }
    protected override async Task<bool> Consume(CreateImportRowsFromWebCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var toProcess = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
            .ConfigureAwait(false);
        if (toProcess == null)
            throw new NotFoundException("Import non trovato");
        

        var actualConfiguration = toProcess.Configuration;
        foreach (ImportRowDto dto in evt.Rows)
        {
            ImportTransaction transaction = new ImportTransaction();

        transaction.Import = toProcess;


        transaction.InTransactionDate = dto.TransactionDate.ToString();
            
        transaction.InDepositAmount = dto.DepositAmount?.ToString();
        transaction.InWithdrawalAmount = dto.WithdrawalAmount?.ToString();
        transaction.InAmount = dto.Amount?.ToString();

        transaction.InCategoryName = dto.CategoryName;
        transaction.InSubCategoryName = dto.SubCategoryName;
        transaction.Description = dto.Description;
        transaction.InCurrency = dto.Currency;
        transaction.InBeneficiary = dto.Beneficiary;
        transaction.InType = dto.Type;
        transaction.InStatus = dto.Status;

        transaction.ImportTransactionStatus = ImportTransactionStatus.New;

        transaction.Trace(currentUser);
        await session.SaveAsync(transaction, cancellationToken).ConfigureAwait(false);
        }

        _storeCommitAction.EnqueueJob<ServiceJob>(j => j.FinalizeImportFromWeb_step1(toProcess.Id, evt.CurrentUserId, cancellationToken));
        return true;
    }
}