using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class GetAssemblyByIdCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public int AssemblyId { get; set; }

    public GetAssemblyByIdCommand() { }
    public GetAssemblyByIdCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
