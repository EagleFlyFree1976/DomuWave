using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.PrivateThread;
using DomuWave.Services.Dto.PrivateThread;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;
using Models = DomuWave.Services.Models;

namespace DomuWave.Services.Consumers.PrivateThread;

public class GetPrivateThreadsByCondominiumCommandConsumer : InMemoryConsumerBase<GetPrivateThreadsByCondominiumCommand, IList<PrivateThreadReadDto>>
{
    private readonly IPrivateThreadService  _threadService;
    private readonly IPrivateMessageService _messageService;
    private readonly IUserService           _userService;

    public GetPrivateThreadsByCondominiumCommandConsumer(ISessionFactoryProvider sp, IPrivateThreadService threadService, IPrivateMessageService messageService, IUserService userService)
        : base(sp) { _threadService = threadService; _messageService = messageService; _userService = userService; }

    protected override async Task<IList<PrivateThreadReadDto>> Consume(GetPrivateThreadsByCondominiumCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var threads = await _threadService.GetByCondominiumAsync(command.CondominiumId, currentUser, ct).ConfigureAwait(false);
        var result = new List<PrivateThreadReadDto>();
        foreach (var thread in threads)
        {
            var messages = await session.Query<PrivateMessage>()
                .Where(m => m.Thread.Id == thread.Id && !m.IsDeleted)
                .OrderByDescending(m => m.CreationDate)
                .ToListAsync(ct).ConfigureAwait(false);
            var count = messages.Count;
            var unread = messages.Count(m => !m.IsReadByRecipient && m.SenderUserId != (long)currentUser.Id);
            var last = messages.FirstOrDefault();
            result.Add(thread.ToReadDto(count, unread, last?.Name, last?.CreationDate));
        }
        return result;
    }
}

public class GetOrCreatePrivateThreadCommandConsumer : InMemoryConsumerBase<GetOrCreatePrivateThreadCommand, PrivateThreadReadDto>
{
    private readonly IPrivateThreadService _threadService;
    private readonly IUserService          _userService;

    public GetOrCreatePrivateThreadCommandConsumer(ISessionFactoryProvider sp, IPrivateThreadService threadService, IUserService userService)
        : base(sp) { _threadService = threadService; _userService = userService; }

    protected override async Task<PrivateThreadReadDto> Consume(GetOrCreatePrivateThreadCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        var existing = await _threadService.GetByCondominiumAndUserAsync(command.CondominiumId, command.CondominioUserId, currentUser, ct).ConfigureAwait(false);
        if (existing != null)
            return existing.ToReadDto();

        var condominium = await session.GetAsync<Models.Condominium>(command.CondominiumId, ct).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        var condominioUser = await _userService.GetByIdAsync((int)command.CondominioUserId, ct).ConfigureAwait(false)
                             ?? throw new NotFoundException("Utente non trovato.");

        var thread = new Models.PrivateThread
        {
            Condominium            = condominium,
            Tenant                 = condominium.Tenant,
            CondominioUserId = command.CondominioUserId,
            Name             = condominioUser.FullName,
        };
        thread.Trace(currentUser);
        await session.SaveAsync(thread, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return thread.ToReadDto();
    }
}

public class GetPrivateMessagesByThreadCommandConsumer : InMemoryConsumerBase<GetPrivateMessagesByThreadCommand, IList<PrivateMessageReadDto>>
{
    private readonly IPrivateMessageService _messageService;
    private readonly IUserService           _userService;

    public GetPrivateMessagesByThreadCommandConsumer(ISessionFactoryProvider sp, IPrivateMessageService messageService, IUserService userService)
        : base(sp) { _messageService = messageService; _userService = userService; }

    protected override async Task<IList<PrivateMessageReadDto>> Consume(GetPrivateMessagesByThreadCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var messages = await _messageService.GetByThreadAsync(command.ThreadId, currentUser, ct).ConfigureAwait(false);
        return messages.Select(m => m.ToReadDto()).ToList();
    }
}

public class CreatePrivateMessageCommandConsumer : InMemoryConsumerBase<CreatePrivateMessageCommand, PrivateMessageReadDto>
{
    private readonly IPrivateMessageService _messageService;
    private readonly IUserService           _userService;

    public CreatePrivateMessageCommandConsumer(ISessionFactoryProvider sp, IPrivateMessageService messageService, IUserService userService)
        : base(sp) { _messageService = messageService; _userService = userService; }

    protected override async Task<PrivateMessageReadDto> Consume(CreatePrivateMessageCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Body))
            throw new ValidatorException("Il messaggio non può essere vuoto.");

        var thread = await session.GetAsync<Models.PrivateThread>(command.Dto.ThreadId, ct).ConfigureAwait(false)
                     ?? throw new NotFoundException("Conversazione non trovata.");

        var entity = command.Dto.ToEntity(thread, thread.Tenant, (long)currentUser.Id, currentUser.FullName);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, ct).ConfigureAwait(false);

        thread.Trace(currentUser);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class MarkThreadMessagesReadCommandConsumer : InMemoryConsumerBase<MarkThreadMessagesReadCommand, bool>
{
    private readonly IUserService _userService;

    public MarkThreadMessagesReadCommandConsumer(ISessionFactoryProvider sp, IUserService userService)
        : base(sp) => _userService = userService;

    protected override async Task<bool> Consume(MarkThreadMessagesReadCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var unread = await session.Query<PrivateMessage>()
            .Where(m => m.Thread.Id == command.ThreadId && !m.IsReadByRecipient && m.SenderUserId != command.ReaderUserId && !m.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);
        foreach (var msg in unread)
        {
            msg.IsReadByRecipient = true;
            msg.Trace(currentUser);
        }
        if (unread.Any())
            await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }
}
