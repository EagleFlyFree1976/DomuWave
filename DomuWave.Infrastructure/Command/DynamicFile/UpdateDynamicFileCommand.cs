using DomuWave.Services.Dto.DynamicFile;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.DynamicFile;

public class UpdateDynamicFileCommand : BaseCommand, IQuery<DynamicFileReadDto>
{
    public int                 Id  { get; set; }
    public UpdateDynamicFileDto Dto { get; set; }

    public UpdateDynamicFileCommand() { }

    public UpdateDynamicFileCommand(int currentUserId, int id, UpdateDynamicFileDto dto)
        : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
