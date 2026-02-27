using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class CreateRealEstateUnitCommand : BaseCommand, IQuery<Models.RealEstateUnit>
{
    public Models.RealEstateUnit Entity { get; set; }

    public CreateRealEstateUnitCommand() { }

    public CreateRealEstateUnitCommand(int currentUserId) : base(currentUserId) { }
    public CreateRealEstateUnitCommand(int currentUserId, Models.RealEstateUnit entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
