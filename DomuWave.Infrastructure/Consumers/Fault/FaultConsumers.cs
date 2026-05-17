using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Fault;
using DomuWave.Services.Dto.Fault;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;
using Models = DomuWave.Services.Models;

namespace DomuWave.Services.Consumers.Fault;

public class GetFaultsByCondominiumCommandConsumer : InMemoryConsumerBase<GetFaultsByCondominiumCommand, IList<FaultReadDto>>
{
    private readonly IFaultService        _faultService;
    private readonly IFaultMessageService _messageService;
    private readonly IUserService         _userService;

    public GetFaultsByCondominiumCommandConsumer(ISessionFactoryProvider sp, IFaultService faultService, IFaultMessageService messageService, IUserService userService)
        : base(sp) { _faultService = faultService; _messageService = messageService; _userService = userService; }

    protected override async Task<IList<FaultReadDto>> Consume(GetFaultsByCondominiumCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var faults = await _faultService.GetByCondominiumAsync(command.CondominiumId, currentUser, ct).ConfigureAwait(false);
        var result = new List<FaultReadDto>();
        foreach (var fault in faults)
        {
            var count = await session.Query<FaultMessage>()
                .CountAsync(m => m.Fault.Id == fault.Id && !m.IsDeleted, ct).ConfigureAwait(false);
            result.Add(fault.ToReadDto(count));
        }
        return result;
    }
}

public class GetFaultByIdCommandConsumer : InMemoryConsumerBase<GetFaultByIdCommand, FaultReadDto>
{
    private readonly IFaultService _faultService;
    private readonly IUserService  _userService;

    public GetFaultByIdCommandConsumer(ISessionFactoryProvider sp, IFaultService faultService, IUserService userService)
        : base(sp) { _faultService = faultService; _userService = userService; }

    protected override async Task<FaultReadDto> Consume(GetFaultByIdCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var fault = await _faultService.GetByIdAsync(command.Id, currentUser, ct).ConfigureAwait(false)
                    ?? throw new NotFoundException("Segnalazione non trovata.");
        var count = await session.Query<FaultMessage>()
            .CountAsync(m => m.Fault.Id == fault.Id && !m.IsDeleted, ct).ConfigureAwait(false);
        return fault.ToReadDto(count);
    }
}

public class CreateFaultCommandConsumer : InMemoryConsumerBase<CreateFaultCommand, FaultReadDto>
{
    private readonly IFaultService _faultService;
    private readonly IUserService  _userService;

    public CreateFaultCommandConsumer(ISessionFactoryProvider sp, IFaultService faultService, IUserService userService)
        : base(sp) { _faultService = faultService; _userService = userService; }

    protected override async Task<FaultReadDto> Consume(CreateFaultCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.Description))
            throw new ValidatorException("La descrizione è obbligatoria.");

        var condominium = await session.GetAsync<Models.Condominium>(command.Dto.CondominiumId, ct).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        RealEstateUnit? unit = null;
        if (command.Dto.UnitId.HasValue)
            unit = await session.GetAsync<RealEstateUnit>(command.Dto.UnitId.Value, ct).ConfigureAwait(false);

        var status = await session.GetAsync<FaultStatus>(0, ct).ConfigureAwait(false)
                     ?? throw new NotFoundException("Stato non trovato.");

        var entity = command.Dto.ToEntity(condominium, unit, status, condominium.Tenant, (long)currentUser.Id, currentUser.FullName);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class UpdateFaultStatusCommandConsumer : InMemoryConsumerBase<UpdateFaultStatusCommand, FaultReadDto>
{
    private readonly IFaultService _faultService;
    private readonly IUserService  _userService;

    public UpdateFaultStatusCommandConsumer(ISessionFactoryProvider sp, IFaultService faultService, IUserService userService)
        : base(sp) { _faultService = faultService; _userService = userService; }

    protected override async Task<FaultReadDto> Consume(UpdateFaultStatusCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var fault = await _faultService.GetByIdAsync(command.Id, currentUser, ct).ConfigureAwait(false)
                    ?? throw new NotFoundException("Segnalazione non trovata.");
        var status = await session.GetAsync<FaultStatus>(command.Dto.StatusId, ct).ConfigureAwait(false)
                     ?? throw new ValidatorException("Stato non valido.");
        fault.Status = status;
        fault.Trace(currentUser);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return fault.ToReadDto();
    }
}

public class DeleteFaultCommandConsumer : InMemoryConsumerBase<DeleteFaultCommand, bool>
{
    private readonly IFaultService _faultService;
    private readonly IUserService  _userService;

    public DeleteFaultCommandConsumer(ISessionFactoryProvider sp, IFaultService faultService, IUserService userService)
        : base(sp) { _faultService = faultService; _userService = userService; }

    protected override async Task<bool> Consume(DeleteFaultCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        return await _faultService.DeleteAsync(command.Id, currentUser, ct).ConfigureAwait(false);
    }
}

public class GetFaultMessagesByFaultCommandConsumer : InMemoryConsumerBase<GetFaultMessagesByFaultCommand, IList<FaultMessageReadDto>>
{
    private readonly IFaultMessageService _messageService;
    private readonly IUserService         _userService;

    public GetFaultMessagesByFaultCommandConsumer(ISessionFactoryProvider sp, IFaultMessageService messageService, IUserService userService)
        : base(sp) { _messageService = messageService; _userService = userService; }

    protected override async Task<IList<FaultMessageReadDto>> Consume(GetFaultMessagesByFaultCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var messages = await _messageService.GetByFaultAsync(command.FaultId, currentUser, ct).ConfigureAwait(false);
        return messages.Select(m => m.ToReadDto()).ToList();
    }
}

public class CreateFaultMessageCommandConsumer : InMemoryConsumerBase<CreateFaultMessageCommand, FaultMessageReadDto>
{
    private readonly IFaultMessageService _messageService;
    private readonly IUserService         _userService;

    public CreateFaultMessageCommandConsumer(ISessionFactoryProvider sp, IFaultMessageService messageService, IUserService userService)
        : base(sp) { _messageService = messageService; _userService = userService; }

    protected override async Task<FaultMessageReadDto> Consume(CreateFaultMessageCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Body))
            throw new ValidatorException("Il messaggio non può essere vuoto.");

        var fault = await session.GetAsync<Models.Fault>(command.Dto.FaultId, ct).ConfigureAwait(false)
                    ?? throw new NotFoundException("Segnalazione non trovata.");

        var entity = command.Dto.ToEntity(fault, fault.Tenant, (long)currentUser.Id, currentUser.FullName);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
