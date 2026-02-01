using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.DTO;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book
{
    /// <summary>
    /// Crea una nuova tipologia conto
    /// </summary>
    public class CreateAccountTypeCommand : BaseCommand,IQuery<AccountTypeReadDto>
    {
        public CreateAccountTypeCommand()
        {
        }

        public CreateAccountTypeCommand(int currentUserId) : base(currentUserId)
        {
        }

        public AccountTypeCreateUpdateDto Item { get; set; }
        
    }
}
