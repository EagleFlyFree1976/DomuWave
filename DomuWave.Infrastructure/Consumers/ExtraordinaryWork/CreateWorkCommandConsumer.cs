using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Dto.ExtraordinaryWork;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.ExtraordinaryWork;

public class CreateWorkCommandConsumer
    : InMemoryConsumerBase<CreateWorkCommand, ExtraordinaryWorkReadDto>
{
    private readonly IExtraordinaryWorkService _workService;
    private readonly ICondominiumService       _condominiumService;
    private readonly IUserService              _userService;

    public CreateWorkCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExtraordinaryWorkService workService,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    { _workService = workService; _condominiumService = condominiumService; _userService = userService; }

    protected override async Task<ExtraordinaryWorkReadDto> Consume(
        CreateWorkCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo del lavoro è obbligatorio");

        var condominium = await _condominiumService.GetByIdAsync(command.Dto.CondominiumId, currentUser, cancellationToken).ConfigureAwait(false);
        if (condominium == null)
            throw new NotFoundException("Condominio non trovato");

        var entity  = command.Dto.ToEntity(condominium);
        var created = await _workService.CreateAsync(entity, currentUser, cancellationToken).ConfigureAwait(false);
        return created.ToReadDto();
    }
}
