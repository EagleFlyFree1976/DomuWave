using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Models.Dto;
using CPQ.Core.DTO;
using NHibernate.Hql.Ast.ANTLR.Tree;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book
{
    /// <summary>
    /// Crea un nuovo book con im parametri impostati
    /// </summary>
    public class CreateAccountCommand : BaseBookRelatedCommand,IQuery<AccountReadDto>
    {
        public CreateAccountCommand()
        {
        }

        public CreateAccountCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
        {
        }

        public AccountCreateDto CreateDto { get; set; }
        
    }
}
