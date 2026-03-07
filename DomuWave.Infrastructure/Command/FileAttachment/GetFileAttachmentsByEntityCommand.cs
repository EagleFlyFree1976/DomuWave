using DomuWave.Services.Dto.FileAttachment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FileAttachment;

public class GetFileAttachmentsByEntityCommand : BaseCommand, IQuery<IList<FileAttachmentReadDto>>
{
    public string EntityKey { get; set; }
    public int EntityId { get; set; }

    public GetFileAttachmentsByEntityCommand() { }

    public GetFileAttachmentsByEntityCommand(int currentUserId, string entityKey, int entityId)
        : base(currentUserId)
    {
        EntityKey = entityKey;
        EntityId  = entityId;
    }
}
