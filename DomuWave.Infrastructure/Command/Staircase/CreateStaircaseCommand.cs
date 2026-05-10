using DomuWave.Services.Dto.Staircase;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Staircase;

public class CreateStaircaseCommand : BaseCommand, IQuery<StaircaseReadDto>
{
    public CreateStaircaseDto Dto { get; set; }

    public CreateStaircaseCommand() { }
    public CreateStaircaseCommand(int currentUserId, CreateStaircaseDto dto) : base(currentUserId)
        => Dto = dto;
}
