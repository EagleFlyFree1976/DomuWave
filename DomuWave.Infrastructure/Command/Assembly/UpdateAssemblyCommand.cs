using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class UpdateAssemblyCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public int               Id  { get; set; }
    public UpdateAssemblyDto Dto { get; set; }

    public UpdateAssemblyCommand() { }
    public UpdateAssemblyCommand(int currentUserId, int id, UpdateAssemblyDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
