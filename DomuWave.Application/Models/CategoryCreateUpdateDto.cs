namespace DomuWave.Application.Models;

public class CategoryCreateUpdateDto
{
    public string Name { get; set; }
    public string Description { get; set; }

    public long? ParentCategoryId { get; set; }
}