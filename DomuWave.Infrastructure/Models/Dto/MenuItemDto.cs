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
}