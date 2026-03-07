using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateMillesimalTableCommandConsumer : InMemoryConsumerBase<CreateMillesimalTableCommand, MillesimalTableReadDto>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService            _userService;

    public CreateMillesimalTableCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        IMillesimalTableService  millesimalTableService,
        IUserService             userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService            = userService;
    }

    protected override async Task<MillesimalTableReadDto> Consume(
        CreateMillesimalTableCommand command,
        IMediationContext            mediationContext,
        CancellationToken            cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;
        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(x => x.Id == dto.CondominiumId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var entity  = dto.ToEntity(condominium);
        var created = await _millesimalTableService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
