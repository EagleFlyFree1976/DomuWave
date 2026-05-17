using DomuWave.Services.Dto.NotificationAttachment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.NotificationAttachment;

public class GetNotificationAttachmentsCommand : BaseCommand, IQuery<IList<NotificationAttachmentReadDto>>
{
    public long NotificationId { get; set; }
    public GetNotificationAttachmentsCommand() { }
    public GetNotificationAttachmentsCommand(int currentUserId, long notificationId) : base(currentUserId)
        => NotificationId = notificationId;
}

public class AddNotificationAttachmentCommand : BaseCommand, IQuery<NotificationAttachmentReadDto>
{
    public long NotificationId { get; set; }
    public int  DocumentId     { get; set; }
    public AddNotificationAttachmentCommand() { }
    public AddNotificationAttachmentCommand(int currentUserId, long notificationId, int documentId) : base(currentUserId)
    { NotificationId = notificationId; DocumentId = documentId; }
}

public class RemoveNotificationAttachmentCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public RemoveNotificationAttachmentCommand() { }
    public RemoveNotificationAttachmentCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
