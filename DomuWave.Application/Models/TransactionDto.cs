
using DomuWave.Services.Models;

namespace DomuWave.Application.Models;

public class TransactionDto : TransactionCreateUpdateDto
{
 

     

}



public class TransactionMassiveUpdateDto : DomuWave.Services.Models.Dto.Transaction.TransactionMassiveUpdateDto
{
    public IList<long> TransactionIds { get; set; } = new List<long>();
}