using DomuWave.Services.Dto.MillesimalTable;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class GetMillesimalTableByCodeCommand : BaseCommand, IQuery<MillesimalTableReadDto>
{
    public int CondominiumId { get; set; }
    public string Code { get; set; }

    public GetMillesimalTableByCodeCommand() { }

    public GetMillesimalTableByCodeCommand(int currentUserId) : base(currentUserId) { }
    public GetMillesimalTableByCodeCommand(int currentUserId, int condominiumId, string code) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Code = code;
    }
}
