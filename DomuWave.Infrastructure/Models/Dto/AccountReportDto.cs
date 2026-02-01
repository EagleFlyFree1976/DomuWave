using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto;

public class AccountReportDto : TraceEntityDTO<long>
{
    public  int AccountId { get; set; }
 

    public virtual string ReportKey { get; set; }


    public virtual decimal? ReportValue { get; set; }

    public virtual DateTime? StartDate { get; set; }
    public virtual DateTime? EndDate { get; set; }
}