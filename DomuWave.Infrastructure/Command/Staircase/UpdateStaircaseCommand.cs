using DomuWave.Services.Dto.Staircase;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Staircase;

public class UpdateStaircaseCommand : BaseCommand, IQuery<StaircaseReadDto>
{
    public int               Id  { get; set; }
    public UpdateStaircaseDto Dto { get; set; }

    public UpdateStaircaseCommand() { }
    public UpdateStaircaseCommand(int currentUserId, int id, UpdateStaircaseDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
