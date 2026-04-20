using DomuWave.Services.Dto.NotificationTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.NotificationTemplate;

public class GetNotificationTemplatesByCondominiumCommand : BaseCommand, IQuery<IList<NotificationTemplateReadDto>>
{
    public int CondominiumId { get; set; }
    public GetNotificationTemplatesByCondominiumCommand() { }
    public GetNotificationTemplatesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

public class GetNotificationTemplateByIdCommand : BaseCommand, IQuery<NotificationTemplateReadDto?>
{
    public int TemplateId { get; set; }
    public GetNotificationTemplateByIdCommand() { }
    public GetNotificationTemplateByIdCommand(int currentUserId, int templateId) : base(currentUserId)
        => TemplateId = templateId;
}

public class CreateNotificationTemplateCommand : BaseCommand, IQuery<NotificationTemplateReadDto>
{
    public CreateNotificationTemplateDto Dto { get; set; } = null!;
    public CreateNotificationTemplateCommand() { }
    public CreateNotificationTemplateCommand(int currentUserId, CreateNotificationTemplateDto dto) : base(currentUserId)
        => Dto = dto;
}

public class UpdateNotificationTemplateCommand : BaseCommand, IQuery<NotificationTemplateReadDto?>
{
    public int                           Id  { get; set; }
    public UpdateNotificationTemplateDto Dto { get; set; } = null!;
    public UpdateNotificationTemplateCommand() { }
    public UpdateNotificationTemplateCommand(int currentUserId, int id, UpdateNotificationTemplateDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}

public class DeleteNotificationTemplateCommand : BaseCommand, IQuery<bool>
{
    public int TemplateId { get; set; }
    public DeleteNotificationTemplateCommand() { }
    public DeleteNotificationTemplateCommand(int currentUserId, int templateId) : base(currentUserId)
        => TemplateId = templateId;
}
