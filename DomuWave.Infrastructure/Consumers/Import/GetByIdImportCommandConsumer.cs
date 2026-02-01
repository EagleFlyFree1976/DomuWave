using DomuWave.Services.Command.Import;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Import;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class GetByIdImportCommandConsumer : InMemoryConsumerBase<GetByIdImportCommand, ImportDto>
{
    private IUserService _userService;
    private IImportService _importService;

    public GetByIdImportCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
    }

    protected override async Task<ImportDto> Consume(GetByIdImportCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var importToUpdate = await session.GetAsync<Models.Import.Import>(evt.ImportId, cancellationToken)
            .ConfigureAwait(false);
        if (importToUpdate == null)
            throw new NotFoundException("Import non trovato");

        return importToUpdate.ToDto();
    }
}