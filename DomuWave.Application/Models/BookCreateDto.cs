namespace DomuWave.Application.Models
{
    public class BookCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public int OwnerId { get; set; }
    }
}
