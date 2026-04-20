namespace DomuWave.Services.Dto.BillingGroup;

public class UpdateBillingGroupDto
{
    public string?    Name         { get; set; }
    public string?    ContactEmail { get; set; }
    public string?    ContactPhone { get; set; }
    public string?    Notes        { get; set; }
    public IList<int> UnitIds      { get; set; } = new List<int>();
}
