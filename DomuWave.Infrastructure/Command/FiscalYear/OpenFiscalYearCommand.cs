using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class OpenFiscalYearCommand : BaseCommand, IQuery<FiscalYearReadDto>
{
    public int CondominiumId { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? PreviousFiscalYearId { get; set; }

    public OpenFiscalYearCommand() { }

    public OpenFiscalYearCommand(int currentUserId) : base(currentUserId) { }
    public OpenFiscalYearCommand(int currentUserId, int condominiumId, string code, string description, DateTime startDate, DateTime endDate, int? previousFiscalYearId = null) : base(currentUserId)
    {
        CondominiumId        = condominiumId;
        Code                 = code;
        Description          = description;
        StartDate            = startDate;
        EndDate              = endDate;
        PreviousFiscalYearId = previousFiscalYearId;
    }
}
