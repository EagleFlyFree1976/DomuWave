using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.AdminTask;

public class CreateAdminTaskDto
{
    [Required] public string  Title       { get; set; } = string.Empty;
    public string?  Description           { get; set; }

    public int       PriorityId           { get; set; } = 2; // Media
    public int       StatusId             { get; set; } = 1; // Da fare
    public DateTime? DueDate              { get; set; }

    /// <summary>Id (AuthService) del collaboratore assegnatario; opzionale.</summary>
    public int?      AssignedToUserId     { get; set; }
    /// <summary>Nome dell'assegnatario (risolto dal controller); opzionale.</summary>
    public string?   AssignedToFullName   { get; set; }

    /// <summary>Id dei condomìni collegati (0/1/N).</summary>
    public List<int> CondominiumIds       { get; set; } = new();
}
