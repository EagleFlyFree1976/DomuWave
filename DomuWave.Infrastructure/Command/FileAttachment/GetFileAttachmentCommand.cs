using DomuWave.Services.Dto.FileAttachment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FileAttachment;

public class GetFileAttachmentCommand : BaseCommand, IQuery<FileAttachmentReadDto>
{
    public int AttachmentId { get; set; }

    public GetFileAttachmentCommand() { }

    public GetFileAttachmentCommand(int currentUserId, int attachmentId)
        : base(currentUserId)
    {
        AttachmentId = attachmentId;
    }
}
