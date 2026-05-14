using DomuWave.Services.Dto.Admin;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Admin;

public class AlignHiloCommand : BaseCommand, IQuery<IList<HiloAlignmentResultDto>>
{
    public AlignHiloCommand() { }
    public AlignHiloCommand(int currentUserId) : base(currentUserId) { }
}
