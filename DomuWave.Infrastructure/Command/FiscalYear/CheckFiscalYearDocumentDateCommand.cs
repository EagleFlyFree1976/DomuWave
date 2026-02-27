using DomuWave.Services.Dto.Contabilita.FiscalYear;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class CheckFiscalYearDocumentDateCommand : BaseCommand, IQuery<DocumentDateWarningDto>
{
    public int FiscalYearId { get; set; }
    public DateTime DocumentDate { get; set; }

    public CheckFiscalYearDocumentDateCommand() { }

    public CheckFiscalYearDocumentDateCommand(int currentUserId) : base(currentUserId) { }
    public CheckFiscalYearDocumentDateCommand(int currentUserId, int fiscalYearId, DateTime documentDate) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
        DocumentDate = documentDate;
    }
}
