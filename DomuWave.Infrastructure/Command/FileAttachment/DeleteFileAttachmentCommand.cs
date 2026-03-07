using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FileAttachment;

public class DeleteFileAttachmentCommand : BaseCommand, IQuery<bool>
{
    public int AttachmentId { get; set; }

    public DeleteFileAttachmentCommand() { }

    public DeleteFileAttachmentCommand(int currentUserId, int attachmentId)
        : base(currentUserId)
    {
        AttachmentId = attachmentId;
    }
}
