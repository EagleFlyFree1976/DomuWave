using DomuWave.Services.Dto.PrivateThread;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PrivateThread;

public class GetPrivateThreadsByCondominiumCommand : BaseCommand, IQuery<IList<PrivateThreadReadDto>>
{
    public int CondominiumId { get; set; }
    public GetPrivateThreadsByCondominiumCommand() { }
    public GetPrivateThreadsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

public class GetOrCreatePrivateThreadCommand : BaseCommand, IQuery<PrivateThreadReadDto>
{
    public int  CondominiumId    { get; set; }
    public long CondominioUserId { get; set; }
    public GetOrCreatePrivateThreadCommand() { }
    public GetOrCreatePrivateThreadCommand(int currentUserId, int condominiumId, long condominioUserId) : base(currentUserId)
    { CondominiumId = condominiumId; CondominioUserId = condominioUserId; }
}

public class GetPrivateMessagesByThreadCommand : BaseCommand, IQuery<IList<PrivateMessageReadDto>>
{
    public int ThreadId { get; set; }
    public GetPrivateMessagesByThreadCommand() { }
    public GetPrivateMessagesByThreadCommand(int currentUserId, int threadId) : base(currentUserId) => ThreadId = threadId;
}

public class CreatePrivateMessageCommand : BaseCommand, IQuery<PrivateMessageReadDto>
{
    public CreatePrivateMessageDto Dto { get; set; } = null!;
    public CreatePrivateMessageCommand() { }
    public CreatePrivateMessageCommand(int currentUserId, CreatePrivateMessageDto dto) : base(currentUserId) => Dto = dto;
}

public class MarkThreadMessagesReadCommand : BaseCommand, IQuery<bool>
{
    public int  ThreadId        { get; set; }
    public long ReaderUserId    { get; set; }
    public MarkThreadMessagesReadCommand() { }
    public MarkThreadMessagesReadCommand(int currentUserId, int threadId, long readerUserId) : base(currentUserId)
    { ThreadId = threadId; ReaderUserId = readerUserId; }
}
