using DomuWave.Services.Dto.UnitMillesimal;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitMillesimal;

public class UpdateUnitMillesimalCommand : BaseCommand, IQuery<UnitMillesimalReadDto>
{
    public int                    EntryId { get; set; }
    public UpdateUnitMillesimalDto Dto    { get; set; }

    public UpdateUnitMillesimalCommand() { }
    public UpdateUnitMillesimalCommand(int currentUserId, int entryId, UpdateUnitMillesimalDto dto) : base(currentUserId)
    {
        EntryId = entryId;
        Dto     = dto;
    }
}
