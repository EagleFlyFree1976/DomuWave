using DomuWave.Services.Dto.Condominium;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class GetCondominiumSetupStatusCommand : BaseCommand, IQuery<CondominiumSetupStatusDto>
{
    public int CondominiumId { get; set; }

    public GetCondominiumSetupStatusCommand() { }

    public GetCondominiumSetupStatusCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
