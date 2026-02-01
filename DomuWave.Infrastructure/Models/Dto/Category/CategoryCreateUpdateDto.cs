namespace DomuWave.Services.Models.Dto.Category;

public class CategoryCreateUpdateDto
{
    public long BookId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long? ParentCategoryId { get; set; }
}