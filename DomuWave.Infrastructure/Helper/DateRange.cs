namespace DomuWave.Services.Helper;

public class DateRange
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsEmpty()
    {
        return !StartDate.HasValue && !EndDate.HasValue;
    }
}