using DomuWave.Services.Command.Import;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class FullImportFileCommandConsumer : InMemoryConsumerBase<FullImportFileCommand, Models.Import.Import>
{
    private IUserService _userService;
    private IImportService _importService;
    private IMediator _mediator;
    public FullImportFileCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService, IMediator mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
        _mediator = mediator;
    }


 
    protected override async Task<Models.Import.Import> Consume(FullImportFileCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        UpdateImportFileCommand updateImportFileCommand  = new UpdateImportFileCommand()
        {
            BookId = evt.BookId,
            CurrentUserId = evt.CurrentUserId,
            FileName = evt.FileName,
            HasHeader = evt.HasHeader,
            ImportId = evt.ImportId,
            TargetAccountId = evt.TargetAccountId,
            csvStream = evt.csvStream
        };

        Models.Import.Import import = await _mediator.GetResponse(updateImportFileCommand, cancellationToken)
            .ConfigureAwait(false);

        ExtractImportCommand extractImportCommand = new ExtractImportCommand()
        {
            BookId = evt.BookId, CurrentUserId = evt.CurrentUserId, ImportId = import.Id
        };
        await _mediator.GetResponse(extractImportCommand, cancellationToken)
            .ConfigureAwait(false);

        ValidateImportCommand validateImportCommand = new ValidateImportCommand()
        {
            BookId = evt.BookId,
            CurrentUserId = evt.CurrentUserId,
            ImportId = import.Id
        };
        await _mediator.GetResponse(validateImportCommand, cancellationToken)
            .ConfigureAwait(false);


        FinalizeImportCommand finalizeImportCommand = new FinalizeImportCommand()
        {
            BookId = evt.BookId,
            CurrentUserId = evt.CurrentUserId,
            ImportId = import.Id
        };
        await _mediator.GetResponse(finalizeImportCommand, cancellationToken)
            .ConfigureAwait(false);




        return import;

    }
}