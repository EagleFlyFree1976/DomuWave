namespace DomuWave.Services.Dto.Condominium;

/// <summary>
/// Stato complessivo di configurazione di un condominio.
/// Restituito da GET /api/condominiums/{id}/setup-status.
/// </summary>
public class CondominiumSetupStatusDto
{
    public int CondominiumId { get; set; }

    public SetupSectionDto Units            { get; set; } = new();
    public SetupSectionDto Occupants        { get; set; } = new();
    public SetupSectionDto ChartOfAccounts  { get; set; } = new();
    public SetupSectionDto MillesimalTables { get; set; } = new();
    public SetupSectionDto FiscalYear       { get; set; } = new();
    public SetupSectionDto Budget           { get; set; } = new();

    /// <summary>Numero di sezioni con status OK.</summary>
    public int CompletedSections =>
        new[] { Units, Occupants, ChartOfAccounts, MillesimalTables, FiscalYear, Budget }
            .Count(s => s.Status == SetupSectionStatus.Ok);

    public int TotalSections => 6;
}

public class SetupSectionDto
{
    public SetupSectionStatus Status  { get; set; } = SetupSectionStatus.Error;
    public List<SetupCheckDto> Checks { get; set; } = new();
}

public class SetupCheckDto
{
    public bool   IsOk    { get; set; }
    public bool   IsWarn  { get; set; }
    public string Label   { get; set; } = string.Empty;
    public string? Detail { get; set; }
}

public enum SetupSectionStatus
{
    Ok    = 0,
    Warn  = 1,
    Error = 2,
    Na    = 3,
}
