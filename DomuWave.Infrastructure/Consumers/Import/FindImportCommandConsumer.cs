using DomuWave.Services.Command.Import;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Import;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class FindImportCommandConsumer : InMemoryConsumerBase<FindImportCommand, IList<ImportMinDto>>
{
    private IUserService _userService;
    private IImportService _importService;

    public FindImportCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IImportService importService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _importService = importService;
    }

    protected override async Task<IList<ImportMinDto>> Consume(FindImportCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var imports = await session.Query<Models.Import.Import>().FilterByBook(evt.BookId).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (imports == null)
            throw new NotFoundException("Import non trovato");

        return imports.Select(i=>i.ToMinDto()).ToList();
    }
}