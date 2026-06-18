using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.AdminTask;

public class AdminTaskCondominiumDto
{
    public int    CondominiumId   { get; set; }
    public string CondominiumName { get; set; } = string.Empty;
}

public class AdminTaskReadDto : TraceEntityDTO<int>
{
    public string    Title              { get; set; } = string.Empty;
    public string?   Description        { get; set; }

    public int       PriorityId         { get; set; }
    public string    PriorityName       { get; set; } = string.Empty;
    public int       StatusId           { get; set; }
    public string    StatusName         { get; set; } = string.Empty;

    public DateTime? DueDate            { get; set; }

    public int?      AssignedToUserId   { get; set; }
    public string?   AssignedToFullName { get; set; }

    public List<AdminTaskCondominiumDto> Condominiums { get; set; } = new();
}
