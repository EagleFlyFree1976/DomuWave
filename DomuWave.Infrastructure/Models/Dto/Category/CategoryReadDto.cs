namespace DomuWave.Services.Models.Dto.Category;


public class CategoryMinReadDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public CategoryMinReadDto Parent { get; set; }

    public string Description
    {
        get
        {
            if (Parent == null)
            {
                return $"{Name}";
            }
            else
            {
                return $"{Parent.Description}/{Name}";
            }
        }
    }

}

public class CategoryReadDto : BookEntityDto<long>
{
    public bool Enabled { get; set; }

    public CategoryMinReadDto Parent { get; set; }

    public string Description
    {
        get
        {
            if (Parent == null)
            {
                return $"{Name}";
            }
            else
            {
                return $"{Parent.Description}/{Name}";
            }
        }
    }
}