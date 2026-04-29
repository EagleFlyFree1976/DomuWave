using DomuWave.Services.Dto.CommunicationNotification;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CommunicationNotification;

public class GetNotificationsByCommunicationCommand : BaseCommand, IQuery<IList<CommunicationNotificationReadDto>>
{
    public int CommunicationId { get; set; }
    public GetNotificationsByCommunicationCommand() { }
    public GetNotificationsByCommunicationCommand(int currentUserId, int communicationId) : base(currentUserId)
        => CommunicationId = communicationId;
}

public class GenerateCommunicationNotificationsCommand : BaseCommand, IQuery<IList<CommunicationNotificationReadDto>>
{
    public int                      CommunicationId { get; set; }
    public GenerateNotificationsDto Dto             { get; set; } = null!;
    public GenerateCommunicationNotificationsCommand() { }
    public GenerateCommunicationNotificationsCommand(int currentUserId, int communicationId, GenerateNotificationsDto dto) : base(currentUserId)
    {
        CommunicationId = communicationId;
        Dto             = dto;
    }
}

public class SendEmailNotificationsCommand : BaseCommand, IQuery<SendNotificationsResultDto>
{
    public int CommunicationId { get; set; }
    public SendEmailNotificationsCommand() { }
    public SendEmailNotificationsCommand(int currentUserId, int communicationId) : base(currentUserId)
        => CommunicationId = communicationId;
}

public class MarkNotificationPrintedCommand : BaseCommand, IQuery<CommunicationNotificationReadDto>
{
    public long NotificationId { get; set; }
    public MarkNotificationPrintedCommand() { }
    public MarkNotificationPrintedCommand(int currentUserId, long notificationId) : base(currentUserId)
        => NotificationId = notificationId;
}

public class MarkNotificationSentCommand : BaseCommand, IQuery<CommunicationNotificationReadDto>
{
    public long                      NotificationId { get; set; }
    public UpdateNotificationStatusDto Dto          { get; set; } = new();
    public MarkNotificationSentCommand() { }
    public MarkNotificationSentCommand(int currentUserId, long notificationId, UpdateNotificationStatusDto dto) : base(currentUserId)
    {
        NotificationId = notificationId;
        Dto            = dto;
    }
}

public class MarkNotificationDeliveredCommand : BaseCommand, IQuery<CommunicationNotificationReadDto>
{
    public long                      NotificationId { get; set; }
    public UpdateNotificationStatusDto Dto          { get; set; } = new();
    public MarkNotificationDeliveredCommand() { }
    public MarkNotificationDeliveredCommand(int currentUserId, long notificationId, UpdateNotificationStatusDto dto) : base(currentUserId)
    {
        NotificationId = notificationId;
        Dto            = dto;
    }
}

public class GetNotificationBatchPdfCommand : BaseCommand, IQuery<byte[]>
{
    public int CommunicationId { get; set; }
    public GetNotificationBatchPdfCommand() { }
    public GetNotificationBatchPdfCommand(int currentUserId, int communicationId) : base(currentUserId)
        => CommunicationId = communicationId;
}

public class GetNotificationAttachmentPdfCommand : BaseCommand, IQuery<byte[]>
{
    public long NotificationId { get; set; }
    public GetNotificationAttachmentPdfCommand() { }
    public GetNotificationAttachmentPdfCommand(int currentUserId, long notificationId) : base(currentUserId)
        => NotificationId = notificationId;
}

public class UpdateNotificationTextCommand : BaseCommand, IQuery<CommunicationNotificationReadDto>
{
    public long                    NotificationId { get; set; }
    public UpdateNotificationTextDto Dto          { get; set; } = null!;
    public UpdateNotificationTextCommand() { }
    public UpdateNotificationTextCommand(int currentUserId, long notificationId, UpdateNotificationTextDto dto) : base(currentUserId)
    {
        NotificationId = notificationId;
        Dto            = dto;
    }
}

public class DeleteCommunicationNotificationCommand : BaseCommand, IQuery<bool>
{
    public long NotificationId { get; set; }
    public DeleteCommunicationNotificationCommand() { }
    public DeleteCommunicationNotificationCommand(int currentUserId, long notificationId) : base(currentUserId)
        => NotificationId = notificationId;
}

public class GenerateNotificationsFromFeesCommand : BaseCommand, IQuery<GenerateFromFeesResultDto>
{
    public GenerateNotificationsFromFeesDto Dto { get; set; } = null!;
    public GenerateNotificationsFromFeesCommand() { }
    public GenerateNotificationsFromFeesCommand(int currentUserId, GenerateNotificationsFromFeesDto dto) : base(currentUserId)
        => Dto = dto;
}

public class GenerateFromFeesResultDto
{
    public int CommunicationId         { get; set; }
    public IList<CommunicationNotificationReadDto> Notifications { get; set; } = [];
}

public class SendNotificationsResultDto
{
    public int Sent   { get; set; }
    public int Failed { get; set; }
    public IList<string> Errors { get; set; } = [];
}
