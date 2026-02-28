using DomuWave.Services.Dto.Condominium;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Condominium;

public class UpdateCondominiumCommand : BaseCommand, IQuery<CondominiumReadDto>
{
    public int CondominiumId { get; set; }
    public UpdateCondominiumDto Dto { get; set; }

    public UpdateCondominiumCommand() { }

    public UpdateCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public UpdateCondominiumCommand(int currentUserId, int condominiumId, UpdateCondominiumDto dto) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Dto = dto;
    }
}
