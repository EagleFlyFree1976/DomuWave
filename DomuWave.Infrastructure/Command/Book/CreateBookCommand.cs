using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Models.Dto;
using CPQ.Core.DTO;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book
{
    /// <summary>
    /// Crea un nuovo book con im parametri impostati
    /// </summary>
    public class CreateBookCommand : BaseCommand,IQuery<BookReadDto>
    {
        public CreateBookCommand()
        {
        }

        public CreateBookCommand(int currentUserId) : base(currentUserId)
        {
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
    }
}
