using DomuWave.Services.Dto.CondominiumInstallment;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumInstallment;

public class GetInstallmentByYearAndNumberCommand : BaseCommand, IQuery<CondominiumInstallmentReadDto>
{
    public int CondominiumId { get; set; }
    public int Year { get; set; }
    public int Number { get; set; }

    public GetInstallmentByYearAndNumberCommand() { }

    public GetInstallmentByYearAndNumberCommand(int currentUserId) : base(currentUserId) { }
    public GetInstallmentByYearAndNumberCommand(int currentUserId, int condominiumId, int year, int number) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Year = year;
        Number = number;
    }
}
