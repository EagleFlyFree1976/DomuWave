namespace DomuWave.Services.Dto.Dashboard;

public class DashboardSummaryDto
{
    public bool IsSuperAdmin { get; set; }

    // SuperAdmin view
    public int ActiveTenantsCount { get; set; }
    public int TotalActiveCondominiumsCount { get; set; }

    // Tenant user view
    public int CondominiumsCount { get; set; }
    public int TotalUnitsCount { get; set; }
    public int OpenInstallmentsCount { get; set; }
    public int UnpaidExpensesCount { get; set; }
}
