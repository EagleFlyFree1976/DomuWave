using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.AdminTask;

public class UpdateAdminTaskDto
{
    [Required] public string  Title       { get; set; } = string.Empty;
    public string?  Description           { get; set; }

    public int       PriorityId           { get; set; }
    public int       StatusId             { get; set; }
    public DateTime? DueDate              { get; set; }

    public int?      AssignedToUserId     { get; set; }
    public string?   AssignedToFullName   { get; set; }

    public List<int> CondominiumIds       { get; set; } = new();
}
