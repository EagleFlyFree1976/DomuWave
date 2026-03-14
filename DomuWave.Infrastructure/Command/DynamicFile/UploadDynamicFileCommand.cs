using DomuWave.Services.Dto.DynamicFile;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.DynamicFile;

public class UploadDynamicFileCommand : BaseCommand, IQuery<DynamicFileReadDto>
{
    public Guid                TenantId { get; set; }
    public UploadDynamicFileDto Dto     { get; set; }

    public UploadDynamicFileCommand() { }

    public UploadDynamicFileCommand(int currentUserId, Guid tenantId, UploadDynamicFileDto dto)
        : base(currentUserId)
    {
        TenantId = tenantId;
        Dto      = dto;
    }
}
