using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class CreateWorkCommand : BaseCommand, IQuery<ExtraordinaryWorkReadDto>
{
    public CreateExtraordinaryWorkDto Dto { get; }
    public CreateWorkCommand(int currentUserId, CreateExtraordinaryWorkDto dto) : base(currentUserId)
        => Dto = dto;
}
