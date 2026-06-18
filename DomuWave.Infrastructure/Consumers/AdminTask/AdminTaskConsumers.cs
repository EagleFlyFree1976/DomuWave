using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.AdminTask;
using DomuWave.Services.Dto.AdminTask;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AdminTask;

// ── CREATE ──────────────────────────────────────────────────────────────────
public class CreateAdminTaskCommandConsumer
    : InMemoryConsumerBase<CreateAdminTaskCommand, AdminTaskReadDto>
{
    private readonly IUserService        _userService;
    private readonly ICondominiumService _condominiumService;

    public CreateAdminTaskCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        ICondominiumService condominiumService) : base(sessionFactoryProvider)
    {
        _userService        = userService;
        _condominiumService = condominiumService;
    }

    protected override async Task<AdminTaskReadDto> Consume(
        CreateAdminTaskCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo dell'attività è obbligatorio.");

        var tenant = await session.GetAsync<Models.Tenant>(command.TenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Tenant non trovato.");

        var priority = await session.GetAsync<AdminTaskPriorityLookup>(command.Dto.PriorityId, cancellationToken).ConfigureAwait(false)
            ?? throw new ValidatorException("Priorità non valida.");
        var status = await session.GetAsync<AdminTaskStatusLookup>(command.Dto.StatusId, cancellationToken).ConfigureAwait(false)
            ?? throw new ValidatorException("Stato non valido.");

        var entity = command.Dto.ToEntity(tenant, priority, status);

        var condominiumLookup = await BuildCondominiumLookupAsync(command.TenantId, command.Dto.CondominiumIds, currentUser, cancellationToken).ConfigureAwait(false);
        entity.SyncCondominiums(command.Dto.CondominiumIds, condominiumLookup, currentUser);

        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }

    private async Task<IReadOnlyDictionary<int, Models.Condominium>> BuildCondominiumLookupAsync(
        Guid tenantId, IReadOnlyCollection<int> ids, CPQ.Core.Memberships.IUser currentUser, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<int, Models.Condominium>();
        var actives = await _condominiumService.GetActiveCondominiumsAsync(tenantId, currentUser, ct).ConfigureAwait(false);
        return actives.Where(c => ids.Contains(c.Id)).ToDictionary(c => c.Id, c => c);
    }
}

// ── UPDATE ──────────────────────────────────────────────────────────────────
public class UpdateAdminTaskCommandConsumer
    : InMemoryConsumerBase<UpdateAdminTaskCommand, AdminTaskReadDto>
{
    private readonly IUserService        _userService;
    private readonly IAdminTaskService   _taskService;
    private readonly ICondominiumService _condominiumService;

    public UpdateAdminTaskCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        IAdminTaskService taskService,
        ICondominiumService condominiumService) : base(sessionFactoryProvider)
    {
        _userService        = userService;
        _taskService        = taskService;
        _condominiumService = condominiumService;
    }

    protected override async Task<AdminTaskReadDto> Consume(
        UpdateAdminTaskCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo dell'attività è obbligatorio.");

        var entity = await _taskService.GetByIdWithCondominiumsAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Attività non trovata.");

        var priority = await session.GetAsync<AdminTaskPriorityLookup>(command.Dto.PriorityId, cancellationToken).ConfigureAwait(false)
            ?? throw new ValidatorException("Priorità non valida.");
        var status = await session.GetAsync<AdminTaskStatusLookup>(command.Dto.StatusId, cancellationToken).ConfigureAwait(false)
            ?? throw new ValidatorException("Stato non valido.");

        entity.ApplyUpdate(command.Dto, priority, status);

        var actives = await _condominiumService.GetActiveCondominiumsAsync(entity.Tenant.Id, currentUser, cancellationToken).ConfigureAwait(false);
        var lookup  = actives.Where(c => command.Dto.CondominiumIds.Contains(c.Id)).ToDictionary(c => c.Id, c => c);
        entity.SyncCondominiums(command.Dto.CondominiumIds, lookup, currentUser);

        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── DELETE (soft) ───────────────────────────────────────────────────────────
public class DeleteAdminTaskCommandConsumer
    : InMemoryConsumerBase<DeleteAdminTaskCommand, bool>
{
    private readonly IUserService      _userService;
    private readonly IAdminTaskService _taskService;

    public DeleteAdminTaskCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        IAdminTaskService taskService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _taskService = taskService;
    }

    protected override async Task<bool> Consume(
        DeleteAdminTaskCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return await _taskService.DeleteAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false);
    }
}

// ── COMPLETE (shortcut → StatusId = Completata) ─────────────────────────────
public class CompleteAdminTaskCommandConsumer
    : InMemoryConsumerBase<CompleteAdminTaskCommand, AdminTaskReadDto>
{
    private readonly IUserService      _userService;
    private readonly IAdminTaskService _taskService;

    public CompleteAdminTaskCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        IAdminTaskService taskService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _taskService = taskService;
    }

    protected override async Task<AdminTaskReadDto> Consume(
        CompleteAdminTaskCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await _taskService.GetByIdWithCondominiumsAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Attività non trovata.");

        var completed = await session.GetAsync<AdminTaskStatusLookup>(AdminTaskStatusLookup.Completata, cancellationToken).ConfigureAwait(false);
        entity.Status = completed;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── GET BY ID ───────────────────────────────────────────────────────────────
public class GetAdminTaskByIdCommandConsumer
    : InMemoryConsumerBase<GetAdminTaskByIdCommand, AdminTaskReadDto?>
{
    private readonly IUserService      _userService;
    private readonly IAdminTaskService _taskService;

    public GetAdminTaskByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        IAdminTaskService taskService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _taskService = taskService;
    }

    protected override async Task<AdminTaskReadDto?> Consume(
        GetAdminTaskByIdCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity = await _taskService.GetByIdWithCondominiumsAsync(command.Id, currentUser, cancellationToken).ConfigureAwait(false);
        return entity?.ToReadDto();
    }
}

// ── GET BY TENANT (filtri) ──────────────────────────────────────────────────
public class GetAdminTasksByTenantCommandConsumer
    : InMemoryConsumerBase<GetAdminTasksByTenantCommand, IList<AdminTaskReadDto>>
{
    private readonly IUserService      _userService;
    private readonly IAdminTaskService _taskService;

    public GetAdminTasksByTenantCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        IAdminTaskService taskService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _taskService = taskService;
    }

    protected override async Task<IList<AdminTaskReadDto>> Consume(
        GetAdminTasksByTenantCommand command, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var items = await _taskService.GetByTenantFilteredAsync(
            command.TenantId, command.AssignedToUserId, command.StatusId, command.DueBefore, currentUser, cancellationToken)
            .ConfigureAwait(false);
        return items.Select(t => t.ToReadDto()).ToList();
    }
}
