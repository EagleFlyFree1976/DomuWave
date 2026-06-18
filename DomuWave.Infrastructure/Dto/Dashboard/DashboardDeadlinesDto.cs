namespace DomuWave.Services.Dto.Dashboard;

/// <summary>Una voce di scadenza mostrata nella dashboard (aggregato da più fonti).</summary>
public class DeadlineItemDto
{
    /// <summary>Tipo sorgente: "Task" | "Installment" | "Assembly".</summary>
    public string    Type               { get; set; } = string.Empty;
    public int       Id                 { get; set; }
    public string    Title              { get; set; } = string.Empty;
    public string?   Description        { get; set; }
    public DateTime? DueDate            { get; set; }
    public string?   Status             { get; set; }
    public string?   Priority           { get; set; }
    public int?      CondominiumId      { get; set; }
    public string?   CondominiumName    { get; set; }
    public string?   AssignedToFullName { get; set; }
    /// <summary>Percorso frontend per aprire l'elemento.</summary>
    public string    FrontendLink       { get; set; } = string.Empty;
    /// <summary>"Overdue" se la scadenza è già passata, altrimenti "Upcoming".</summary>
    public string    Urgency            { get; set; } = "Upcoming";
}

public class DashboardDeadlinesDto
{
    /// <summary>Prossime attività (task) con assegnatario e scadenza.</summary>
    public List<DeadlineItemDto> UpcomingTasks { get; set; } = new();

    /// <summary>Tutte le scadenze aggregate (task + rate + assemblee), ordinate per data.</summary>
    public List<DeadlineItemDto> Items { get; set; } = new();
}
