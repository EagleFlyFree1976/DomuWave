namespace DomuWave.Services.Models.Dto.Transaction;

public class TransactionCreateDto : TransactionBaseDto
{
    public IList<string> Tags { get; set; } = new List<string>();

}


public class TransactionUpdateDto : TransactionBaseDto
{
 
}




