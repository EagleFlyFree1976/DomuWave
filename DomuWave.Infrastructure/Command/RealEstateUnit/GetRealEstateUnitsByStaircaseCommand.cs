using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitsByStaircaseCommand : BaseCommand, IQuery<IList<Models.RealEstateUnit>>
{
    public int CondominiumId { get; set; }
    public string Staircase { get; set; }

    public GetRealEstateUnitsByStaircaseCommand() { }

    public GetRealEstateUnitsByStaircaseCommand(int currentUserId) : base(currentUserId) { }
    public GetRealEstateUnitsByStaircaseCommand(int currentUserId, int condominiumId, string staircase) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Staircase = staircase;
    }
}
