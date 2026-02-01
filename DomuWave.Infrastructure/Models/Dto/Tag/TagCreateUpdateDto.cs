using MassTransit.Futures.Contracts;

namespace DomuWave.Services.Models.Dto.Tag;

public class TagCreateUpdateDto
{
    public long BookId { get; set; }
    public string Name { get; set; }
    
}