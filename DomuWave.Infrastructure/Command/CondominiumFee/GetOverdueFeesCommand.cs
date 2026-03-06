using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetOverdueFeesCommand : BaseCommand, IQuery<IList<CondominiumFeeReadDto>>
{
    public int CondominiumId { get; set; }

    public GetOverdueFeesCommand() { }

    public GetOverdueFeesCommand(int currentUserId) : base(currentUserId) { }
    public GetOverdueFeesCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
