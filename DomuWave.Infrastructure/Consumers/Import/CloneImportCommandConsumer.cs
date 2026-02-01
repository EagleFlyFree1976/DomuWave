using DomuWave.Services.Command.Import;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class CloneImportCommandConsumer : InMemoryConsumerBase<CloneImportCommand, Models.Import.Import>
{
    private IUserService _userService;
    private IImportService _importService;
    public CloneImportCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
    }

    protected override async Task<Models.Import.Import> Consume(CloneImportCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        Models.Import.Import source = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
            .ConfigureAwait(false);

        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        if (source == null)
            throw new NotFoundException("Elemento da clonare non trovato");

        Models.Import.Import newImport = new Models.Import.Import();
        newImport.Configuration = source.Configuration;
        newImport.ContentType = source.ContentType;
        newImport.Name = $"{source.Name} cloned";
        newImport.FileData = null;
        if (evt.TargetAccountId.HasValue){
            newImport.TargetAccount =  await session.GetAsync<Models.Account>(evt.TargetAccountId.Value, cancellationToken);
        }
        else
        {
            newImport.TargetAccount = source.TargetAccount;
        }
            newImport.Book = source.Book;
        newImport.Trace(currentUser);

        await session.SaveAsync(newImport, cancellationToken).ConfigureAwait(false);
        return newImport;
    }
}