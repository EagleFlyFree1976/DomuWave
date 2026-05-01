using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class GetAssembliesByCondominiumCommand : BaseCommand, IQuery<IList<AssemblyReadDto>>
{
    public int CondominiumId { get; set; }

    public GetAssembliesByCondominiumCommand() { }
    public GetAssembliesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
