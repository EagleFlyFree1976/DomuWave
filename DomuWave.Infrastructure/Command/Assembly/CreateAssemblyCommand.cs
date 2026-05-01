using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class CreateAssemblyCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public CreateAssemblyDto Dto { get; set; }

    public CreateAssemblyCommand() { }
    public CreateAssemblyCommand(int currentUserId, CreateAssemblyDto dto) : base(currentUserId)
        => Dto = dto;
}
