namespace DomuWave.Application.Models;

/// <summary>
/// Parametri per la ricerca paginata dei Tenant.
/// Mappato direttamente dai query-string del frontend.
/// </summary>
public class TenantPagedQuery
{
    /// <summary>Numero di pagina (1-based).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Elementi per pagina.</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Campo su cui ordinare (es. "name", "code", "isActive").</summary>
    public string SortField { get; set; } = "name";

    /// <summary>true = ascendente, false = discendente.</summary>
    public bool Ascending { get; set; } = true;

    /// <summary>Testo libero: ricerca su Name e Code (null o stringa vuota = nessun filtro).</summary>
    public string Search { get; set; }

    /// <summary>Filtra per stato attivo/inattivo. null = tutti.</summary>
    public bool? IsActive { get; set; }
}