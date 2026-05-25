namespace DomuWave.Services.Models;

public class MenuItemDto
{
    public int Id { get; set; }
    public int? ParentMenuId { get; set; }

    public string Icon { get; set; }
    public string Description { get; set; }

    public string ShortDescription { get; set; } = string.Empty;
    public string Action { get; set; }

    public string AuthorizationCode { get; set; }

    public string Tags { get; set; }

    /// <summary>
    /// Feature code separati da virgola richiesti per visualizzare la voce.
    /// Se null o vuoto, la voce è sempre visibile.
    /// </summary>
    public string? Features { get; set; }

    public int OrderKey { get; set; }
}