using DomuWave.Services.Dto.RealEstateUnit;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class CreateRealEstateUnitCommand : BaseCommand, IQuery<RealEstateUnitReadDto>
{
    public CreateRealEstateUnitDto Dto { get; set; }

    public CreateRealEstateUnitCommand() { }

    public CreateRealEstateUnitCommand(int currentUserId) : base(currentUserId) { }
    public CreateRealEstateUnitCommand(int currentUserId, CreateRealEstateUnitDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
