using CPQ.Core;

namespace DomuWave.Services.Models;

public class AccountReport : TraceEntity<long>
{


    public const string ReportCurrentDayAllInput = "ReportCurrentDayAllInput";
    public const string ReportCurrentDayAllOutput = "ReportCurrentDayAllOutput";
    public const string ReportCurrentDayYTDAllInput = "ReportCurrentDayYTDAllInput";
    public const string ReportCurrentDayYTDAllOutput = "ReportCurrentDayYTDAllOutput";

    public const string ReportLastDayAllInput = "ReportLastDayAllInput";
    public const string ReportLastDayAllOutput = "ReportLastDayAllOutput";
    public const string ReportLastDayYTDAllInput = "ReportLastDayYTDAllInput";
    public const string ReportLastDayYTDAllOutput = "ReportLastDayYTDAllOutput";


    public const string ReportCurrentWeekAllInput = "ReportCurrentWeekAllInput";
    public const string ReportCurrentWeekAllOutput = "ReportCurrentWeekAllOutput";
    public const string ReportCurrentWeekYTDAllInput = "ReportCurrentWeekYTDAllInput";
    public const string ReportCurrentWeekYTDAllOutput = "ReportCurrentWeekYTDAllOutput";

    public const string ReportLastWeekAllInput = "ReportLastWeekAllInput";
    public const string ReportLastWeekAllOutput = "ReportLastWeekAllOutput";
    public const string ReportLastWeekYTDAllInput = "ReportLastWeekYTDAllInput";
    public const string ReportLastWeekYTDAllOutput = "ReportLastWeekYTDAllOutput";


    public const string ReportCurrentMonthAllInput = "ReportCurrentMonthAllInput";
    public const string ReportCurrentMonthAllOutput = "ReportCurrentMonthAllOutput";
    public const string ReportCurrentMonthYTDAllInput = "ReportCurrentMonthYTDAllInput";
    public const string ReportCurrentMonthYTDAllOutput = "ReportCurrentMonthYTDAllOutput";

    public const string ReportLastMonthAllInput = "ReportLastMonthAllInput";
    public const string ReportLastMonthAllOutput = "ReportLastMonthAllOutput";
    public const string ReportLastMonthYTDAllInput = "ReportLastMonthYTDAllInput";
    public const string ReportLastMonthYTDAllOutput = "ReportLastMonthYTDAllOutput";



    public const string ReportCurrentQuarterAllInput = "ReportCurrentQuarterAllInput";
    public const string ReportCurrentQuarterAllOutput = "ReportCurrentQuarterAllOutput";
    public const string ReportCurrentQuarterYTDAllInput = "ReportCurrentQuarterYTDAllInput";
    public const string ReportCurrentQuarterYTDAllOutput = "ReportCurrentQuarterYTDAllOutput";

    public const string ReportLastQuarterAllInput = "ReportLastQuarterAllInput";
    public const string ReportLastQuarterAllOutput = "ReportLastQuarterAllOutput";
    public const string ReportLastQuarterYTDAllInput = "ReportLastQuarterYTDAllInput";
    public const string ReportLastQuarterYTDAllOutput = "ReportLastQuarterYTDAllOutput";


    public const string ReportQuarterQ1AllInput = "ReportQuarterQ1AllInput";
    public const string ReportQuarterQ1AllOutput = "ReportQuarterQ1AllOutput";

    public const string ReportQuarterQ2AllInput = "ReportQuarterQ2AllInput";
    public const string ReportQuarterQ2AllOutput = "ReportQuarterQ2AllOutput";

    public const string ReportQuarterQ3AllInput = "ReportQuarterQ3AllInput";
    public const string ReportQuarterQ3AllOutput = "ReportQuarterQ3AllOutput";


    public const string ReportQuarterQ4AllInput = "ReportQuarterQ4AllInput";
    public const string ReportQuarterQ4AllOutput = "ReportQuarterQ4AllOutput";
    
    
    public const string ReportQuarterQ1YTDAllInput = "ReportQuarterQ1YTDAllInput";
    public const string ReportQuarterQ1YTDAllOutput = "ReportQuarterQ1YTDAllOutput";

    public const string ReportQuarterQ2YTDAllInput = "ReportQuarterQ2YTDAllInput";
    public const string ReportQuarterQ2YTDAllOutput = "ReportQuarterQ2YTDAllOutput";

    public const string ReportQuarterQ3YTDAllInput = "ReportQuarterQ3YTDAllInput";
    public const string ReportQuarterQ3YTDAllOutput = "ReportQuarterQ3YTDAllOutput";


    public const string ReportQuarterQ4YTDAllInput = "ReportQuarterQ4YTDAllInput";
    public const string ReportQuarterQ4YTDAllOutput = "ReportQuarterQ4YTDAllOutput";




    public const string ReportActualBalance = "ReportActualBalance";
    public const string ReportEOMBalance = "ReportEOMBalance ";
    public virtual Account Account { get; set; }
    
    public virtual string ReportKey { get; set; }

    
    public virtual decimal? ReportValue { get; set; }


    public virtual DateTime? StartDate { get; set; }
    public virtual DateTime? EndDate { get; set; }
    public override int GetHashCode()
    {

        return this.Id.GetHashCode();

    }
}