using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class SaveAssemblyMinutesCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public int     AssemblyId   { get; set; }
    public string? BoardMembers { get; set; }
    public string? Minutes      { get; set; }

    public SaveAssemblyMinutesCommand() { }
    public SaveAssemblyMinutesCommand(int currentUserId, int assemblyId, string? boardMembers, string? minutes)
        : base(currentUserId)
    {
        AssemblyId   = assemblyId;
        BoardMembers = boardMembers;
        Minutes      = minutes;
    }
}
