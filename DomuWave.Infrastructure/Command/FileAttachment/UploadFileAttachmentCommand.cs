using DomuWave.Services.Dto.FileAttachment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FileAttachment;

public class UploadFileAttachmentCommand : BaseCommand, IQuery<FileAttachmentReadDto>
{
    public Guid TenantId { get; set; }
    public UploadFileAttachmentDto Dto { get; set; }

    public UploadFileAttachmentCommand() { }

    public UploadFileAttachmentCommand(int currentUserId, Guid tenantId, UploadFileAttachmentDto dto)
        : base(currentUserId)
    {
        TenantId = tenantId;
        Dto      = dto;
    }
}
