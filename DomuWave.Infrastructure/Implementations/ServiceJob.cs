using System.Diagnostics;
using System.Net;
using System.Threading;
using DomuWave.Services.Command.Import;
using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Transaction;
using DomuWave.Services.Models.Import;
using CPQ.Core.Clients;
using CPQ.Core.DTO;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Mapping.ByCode;
using SimpleMediator.Core;

namespace DomuWave.Services.Implementations;

public class ServiceJob : CPQ.Core.Services.ServiceBase
{
    protected IMediator _mediator;
    protected IUserService _userService;
    public ServiceJob(ISessionFactoryProvider sessionFactoryProvider, IMediator mediator, IUserService userService, IStoreCommitAction storeCommitAction) : base(sessionFactoryProvider)
    {
        _mediator = mediator;
        _userService = userService;
        _storeCommitAction = storeCommitAction;
    }

    public async Task RecalculateAccountBalance(long accountId, long bookId, int currentUserId,
        CancellationToken cancellationToken)
    {
        await RecalculateAccountBalance(bookId, currentUserId, cancellationToken, accountId).ConfigureAwait(false);
    }
    public async Task RecalculateAccountBalance(long bookId, int currentUserId, CancellationToken cancellationToken, params long[] accountIdlist)
    {
        try
        {
            IUser currentUser = await _userService.GetByIdAsync(currentUserId, cancellationToken).ConfigureAwait(false);
            foreach (long accountId in accountIdlist)
            {
                
            
            RecalculateAccountBalanceCommand recalculateAccountBalanceCommand = new RecalculateAccountBalanceCommand
            {
                BookId = bookId,
                CurrentUserId = currentUser.Id,
                AccountId = accountId

            };
            await _mediator.PublicAsync(recalculateAccountBalanceCommand, cancellationToken).ConfigureAwait(false);
            }
            await sessionFactoryProvider.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await sessionFactoryProvider.RollbackAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine(e);
            throw;
        }
    }


    public async  Task CalculateAccountBalance(long transactionId, long? prevAccountId, long actualAccountId,
        DateTime? prevTransactionDate, DateTime currentTransactionDate, long bookId, int currentUserId,
        CancellationToken cancellationToken)
    {

        
            
            if (prevAccountId.HasValue)
            {
                await RecalculateAccountBalance(bookId, currentUserId, cancellationToken, actualAccountId, prevAccountId.Value)
                    .ConfigureAwait(false);
            }
            else
            {
                await RecalculateAccountBalance(actualAccountId, bookId, currentUserId, cancellationToken)
                    .ConfigureAwait(false);
        }

        
        
    }

    public async Task FinalizeImportFromWeb_step2(long importId, int currentUserId,
        CancellationToken cancellationToken)
    {

        try
        {
            IUser currentUser = await _userService.GetByIdAsync(currentUserId, cancellationToken).ConfigureAwait(false);

            var import = await session.GetAsync<DomuWave.Services.Models.Import.Import>(importId, cancellationToken).ConfigureAwait(false);

            FinalizeImportCommand command = new FinalizeImportCommand()
            {
                BookId = import.Book.Id,
                CurrentUserId = currentUserId,
                ImportId = importId

            };
            await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);

            await sessionFactoryProvider.CommitAsync(cancellationToken).ConfigureAwait(false);

            _storeCommitAction.DequeueAll(true);
        }
        catch (Exception e)
        {
            await sessionFactoryProvider.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _storeCommitAction.DequeueAll(false);
            Console.WriteLine(e);
            throw;
        }



       



        
    }

    private IStoreCommitAction _storeCommitAction;

    public   async Task FinalizeImportFromWeb_step1(long importId, int currentUserId,
        CancellationToken cancellationToken)
    {
        
        try
        {
            IUser currentUser = await _userService.GetByIdAsync(currentUserId, cancellationToken).ConfigureAwait(false);

            var import = await session.GetAsync<DomuWave.Services.Models.Import.Import>(importId, cancellationToken).ConfigureAwait(false);

            ValidateImportCommand command = new ValidateImportCommand()
                { BookId = import.Book.Id, CurrentUserId = currentUserId, ImportId = importId };
            await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);

            _storeCommitAction.EnqueueJob<ServiceJob>(j => j.FinalizeImportFromWeb_step2(importId, currentUserId, cancellationToken));

            await sessionFactoryProvider.CommitAsync(cancellationToken).ConfigureAwait(false);

            _storeCommitAction.DequeueAll(true);
        }
        catch (Exception e)
        {
            await sessionFactoryProvider.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _storeCommitAction.DequeueAll(false);
            Console.WriteLine(e);
            throw;
        }

    }

    public async Task CalculateAccountBalanceForAllActiveAccount(long bookId, int currentUserId, CancellationToken cancellationToken)
    {

        try
        {
            CalculateAccountBalanceForAllActiveAccountCommand command =
                new CalculateAccountBalanceForAllActiveAccountCommand() { BookId = bookId, CurrentUserId = currentUserId };
            await _mediator.GetResponse(command, cancellationToken).ConfigureAwait(false);

            await sessionFactoryProvider.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await sessionFactoryProvider.RollbackAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine(e);
            throw;
        }
     
    }
}