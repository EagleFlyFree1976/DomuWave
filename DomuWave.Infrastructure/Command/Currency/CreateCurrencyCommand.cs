using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.DTO;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book
{
    /// <summary>
    /// Crea una nuova currency
    /// </summary>
    public class CreateCurrencyCommand : BaseCommand,IQuery<CurrencyReadDto>
    {
        public CreateCurrencyCommand()
        {
        }

        public CreateCurrencyCommand(int currentUserId) : base(currentUserId)
        {
        }

        public CurrencyCreateUpdateDto Item { get; set; }
        
        
    }
}
